using System;

namespace EntityStates
{
    public class StateDef
    {
        public StateDef( Type stateType )
        {
            if( !StateIndexTable.IsValid( stateType ) ) throw new ArgumentException( nameof( stateType ) );

            this.stateType = stateType;
            this.stateName = stateType.AssemblyQualifiedName;
            this.stateIndex = -1;
        }

        public Int16 stateIndex { get; internal set; }
        public String stateName { get; private set; }
        public Type stateType { get; private set; }
        public Boolean valid
        {
            get => this.stateIndex > 0;
        }

        internal StateDef( Type stateType, String overrideName )
        {
            if( !StateIndexTable.IsValid( stateType ) ) throw new ArgumentException( nameof( stateType ) );
            this.stateType = stateType;
            this.stateName = overrideName;
            this.stateIndex = -1;
        }
    }
}
