using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

namespace EntityStates
{
    public static class StateIndexTable
    {
        #region Implementations for existing members
        public static Int32 typeCount
        {
            get => stateDefs.Count;
        }

        public static IList<String> typeNames
        {
            get => (IList<String>)stateDefs.Select<StateDef, String>( ( def ) => def.stateName );
        }

        public static Type IndexToType( Int16 stateTypeIndex )
        {
            return LookupStateDef( stateTypeIndex ).stateType;
        }

        public static Int16 TypeToIndex( Type stateType )
        {
            return LookupStateDef( stateType ).stateIndex;
        }
        #endregion

        public const Int16 maxStates = Int16.MaxValue;


        #region Lookups
        public static StateDef LookupStateDef( Type stateType )
        {
            return typeToStateDef[stateType];
        }
        public static StateDef LookupStateDef( Int16 index )
        {
            return stateDefs[index];
        }
        public static StateDef LookupStateDef( String name )
        {
            return nameToStateDef[name];
        }
        #endregion
        #region TryLookups
        public static Boolean TryLookupStateDef( Type stateType, out StateDef stateDef )
        {
            stateDef = errorReturn;
            if( stateType == null ) return false;
            if( !IsValid( stateType ) ) return false;
            return typeToStateDef.TryGetValue( stateType, out stateDef );

        }
        public static Boolean TryLookupStateDef( Int16 stateIndex, out StateDef stateDef )
        {
            stateDef = errorReturn;
            if( stateIndex < 0 ) return false;
            if( stateIndex > stateDefs.Count ) return false;
            stateDef = stateDefs[stateIndex];
            return true;
        }
        public static Boolean TryLookupStateDef( String name, out StateDef stateDef )
        {
            stateDef = errorReturn;
            if( String.IsNullOrEmpty( name ) )
            {
                Debug.LogWarning( "Trying to look up state with null or empty name" );
            }
            return nameToStateDef.TryGetValue( name, out stateDef );
        }
        #endregion
        #region Adds
        public static void AddState( Type stateType )
        {
            if( !IsValid( stateType ) ) throw new ArgumentException( nameof( stateType ) );
            var ind = (Int16)typeCount;
            if( ind >= maxStates ) throw new OverflowException( String.Format( "Cannot add more than {0} states", maxStates ) );
            if( typeToStateDef.ContainsKey( stateType ) )
            {
                Debug.LogErrorFormat( "Type:\n{0}\nis already registered", stateType.FullName );
                return;
            }

            var def = new StateDef( stateType );
            def.stateIndex = ind;

            stateDefs.Add( def );
            typeToStateDef[stateType] = def;
            nameToStateDef[def.stateName] = def;
            needToUpdateBytes = true;
        }

        public static void AddState( StateDef stateDef )
        {
            if( stateDef == null ) throw new ArgumentNullException( nameof( stateDef ) );
            var ind = (Int16)typeCount;
            if( ind >= maxStates ) throw new OverflowException( String.Format( "Cannot add more than {0} states.", maxStates ) );
            var type = stateDef.stateType;
            if( !IsValid( type ) ) throw new ArgumentException( nameof( stateDef ) );
            if( typeToStateDef.ContainsKey( type ) )
            {
                Debug.LogErrorFormat( "StateDef for state:\n{0}\nis already registered", stateDef.stateName );
                return;
            }
            if( stateDef.valid )
            {
                Debug.LogWarningFormat( "StateDef for state:\n{0}\nalready had an index on registration, it will be overwritten", stateDef.stateName );
            }

            stateDef.stateIndex = ind;
            stateDefs.Add( stateDef );
            typeToStateDef[type] = stateDef;
            nameToStateDef[stateDef.stateName] = stateDef;
            needToUpdateBytes = true;
        }

        #endregion
        #region TryAdds
        public static Boolean TryAddState( Type stateType, out StateDef stateDef )
        {
            stateDef = errorReturn;
            if( !IsValid( stateType ) ) return false;
            var ind = (Int16)typeCount;
            if( ind >= maxStates ) return false;
            if( typeToStateDef.ContainsKey( stateType ) ) return false;

            stateDef = new StateDef( stateType );
            stateDef.stateIndex = ind;
            stateDefs.Add( stateDef );
            typeToStateDef[stateType] = stateDef;
            nameToStateDef[stateDef.stateName] = stateDef;
            needToUpdateBytes = true;
            return true;
        }

        public static Boolean TryAddState( ref StateDef stateDef )
        {
            if( stateDef == null ) return false;
            var ind = (Int16)typeCount;
            if( ind >= maxStates ) return false;
            var type = stateDef.stateType;
            if( !IsValid( type ) ) return false;
            if( typeToStateDef.ContainsKey( type ) ) return false;
            if( stateDef.valid )
            {
                Debug.LogWarningFormat( "StateDef for state:\n{0}\nalready had an index on registration, it will be overwritten", stateDef.stateName );
            }

            stateDef.stateIndex = ind;
            stateDefs.Add( stateDef );
            typeToStateDef[type] = stateDef;
            nameToStateDef[stateDef.stateName] = stateDef;
            needToUpdateBytes = true;
            return true;
        }
        #endregion
        #region Utilities
        public static Boolean IsValid( Type type, Boolean hideLogs = false )
        {
            if( hideLogs )
            {
                return type != null && type.IsSubclassOf( typeof( EntityState ) ) && !type.IsAbstract;
            }
            var ret = true;
            if( type == null )
            {
                Debug.LogErrorFormat( "{0} was null", nameof( type ) );
                ret &= false;
            } else
            {
                if( !type.IsSubclassOf( typeof( EntityState ) ) )
                {
                    Debug.LogErrorFormat( "Type:{0}\ndoes not inherit {1}.", type.FullName, nameof( EntityState ) );
                    ret &= false;
                }
                if( type.IsAbstract )
                {
                    Debug.LogErrorFormat( "Type:{0}\nis abstract.", type.FullName );
                    ret &= false;
                }
            }

            return ret;
        }

        #endregion

        #region Hashes
        //Not sure how useful this would actually be outside of testing. In theory could be used to validate the table is the same across clients
        public static String ComputeTableHash()
        {
            var hasher = MD5.Create();
            return System.Text.Encoding.UTF8.GetString( hasher.ComputeHash( stateDefsBytes ) );
        }
        #endregion

        #region Private side
        static StateIndexTable()
        {
            ror2Assembly = typeof( StateIndexTable ).Assembly;
            CollectVanillaStates();
            var errorState = typeof(EntityStates.Uninitialized);
            errorReturn = new StateDef( errorState, errorState.FullName );
            errorReturn.stateIndex = -1;
        }

        private static StateDef errorReturn;
        private static Assembly ror2Assembly;
        private static List<StateDef> stateDefs;
        private static Dictionary<Type,StateDef> typeToStateDef;
        private static Dictionary<String,StateDef> nameToStateDef;
        private static Boolean needToUpdateBytes = true;

        private static Byte[] stateDefsBytes
        {
            get
            {
                if( needToUpdateBytes )
                {
                    var count = typeCount;
                    var tempList = new List<Byte>( count * 32 );

                    for( Int32 i = 0; i < count; ++i )
                    {
                        var tempBytes = System.Text.Encoding.UTF8.GetBytes( stateDefs[i].stateName );
                        for( Int32 j = 0; j < tempBytes.Length; ++j )
                        {
                            tempList.Add( tempBytes[j] );
                        }
                    }

                    needToUpdateBytes = false;
                }
                return _stateDefBytes;
            }
        }
        private static Byte[] _stateDefBytes;


        private static void CollectVanillaStates()
        {
            var temp = new SortedDictionary<String,Type>(StringComparer.Ordinal);

            var types = ror2Assembly.GetTypes();
            for( Int32 i = 0; i < types.Length; ++i )
            {
                var type = types[i];
                if( IsValid( type, true ) )
                {
                    temp.Add( type.Name, type );
                }
            }

            stateDefs = new List<StateDef>( temp.Select<KeyValuePair<String, Type>, StateDef>( ( kvp ) => new StateDef( kvp.Value, kvp.Value.FullName ) ) );
            typeToStateDef = new Dictionary<Type, StateDef>( stateDefs.Count );
            nameToStateDef = new Dictionary<String, StateDef>( stateDefs.Count );


            for( Int16 i = 0; i < stateDefs.Count; ++i )
            {
                var def = stateDefs[i];
                if( def.valid )
                {
                    Debug.LogWarningFormat( "StateDef for type:\n{0}\nalready has an index, the current index will be discarded.", def.stateName );
                }

                def.stateIndex = i;
                typeToStateDef[def.stateType] = def;
                nameToStateDef[def.stateName] = def;
            }
        }

        #endregion
    }
}
