using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public class ConnectionStatus
    {
        public bool IsConfigurationValid => !Errors.Any();
        public List<ConnectionError> Errors { get; set; } = new List<ConnectionError>();
    }
}
