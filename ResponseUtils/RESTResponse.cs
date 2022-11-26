using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResponseUtils
{
    public class RESTResponse<T>
    {
        public int Status { get; set; }
        private string? _message;
        public string? Message
        {
            get => _message;
            set
            {
                if (_message != value)
                    _message = value;
            }
        }

        public T Data { get; set; }
    }
}
