using Common;
using Common.Translations;

namespace BackupTests
{
    internal class NullDataInput : IDataInput
    {
        public bool Called { get; set; }

        public string Get(string message)
        {
            Called = true;
            return DataInputs.Yes;
        }
    }
}
