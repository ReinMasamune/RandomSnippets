using System;
using UnityEngine;

namespace EntityStates
{
    [Serializable]
    public struct SerializableEntityStateType
    {
        #region Constructors
        public SerializableEntityStateType( String typeName )
        {
            this._typeName = "";
            this.typeName = typeName;
        }
        public SerializableEntityStateType( Int16 stateIndex )
        {
            this._typeName = "";
            this.stateType = StateIndexTable.LookupStateDef( stateIndex ).stateType;
        }
        public SerializableEntityStateType( Type stateType )
        {
            this._typeName = "";
            this.stateType = stateType;
        }
        public SerializableEntityStateType( StateDef stateDef )
        {
            if( stateDef == null ) throw new ArgumentNullException( nameof( stateDef ) );
            if( !stateDef.valid ) throw new ArgumentException( String.Format( "StateDef for state:\n{0}\n was not properly registered in StateIndexTable", stateDef.stateName ), nameof( stateDef ) );
            this._typeName = stateDef.stateName;
        }
        #endregion
        public Type stateType
        {
            get
            {
                return StateIndexTable.LookupStateDef( this._typeName ).stateType;
            }
            set
            {
                if( StateIndexTable.TryLookupStateDef( value, out var def ) )
                {
                    this._typeName = def.stateName;
                } else
                {
                    if( value == null )
                    {
                        Debug.LogErrorFormat( "{0} cannot be set to a null type", nameof( this.stateType ) );
                    } else
                    {
                        Debug.LogErrorFormat( "Unregistered or invalid type:\n{0}", value.FullName );
                    }
                }
            }
        }

        [SerializeField]
        private String _typeName;

        private String typeName
        {
            get
            {
                return this._typeName;
            }
            set
            {
                if( StateIndexTable.TryLookupStateDef( value, out var def ) )
                {
                    this._typeName = def.stateName;
                } else
                {
                    Debug.LogErrorFormat( "Could not find state for name:\n{0}", value );
                }
            }
        }
    }
}
