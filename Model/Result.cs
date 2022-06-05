namespace Model
{
    public interface IBaseResult
    {
        bool Successful { get; set; }
    }
    public interface IResult<ResultType> : IBaseResult
    {
        ResultType ResultContainer { get; set; }
    }

    public class Result : IResult<string>
    {
        private bool _Successful;
        private string _Message;
        public bool Successful { get => _Successful; set => _Successful = value; }
        public string ResultContainer { get => _Message; set => _Message = value; }
        public override string ToString()
        {
            return "Result : " + _Successful + " Message : " + ResultContainer;
        }
    }

    public class ErrorResult : IResult<Exception>
    {
        private bool _Successful;
        private Exception _Exception;
        public bool Successful { get => _Successful; set => _Successful = value; }
        public Exception ResultContainer { get => _Exception; set => _Exception = value; }
        public override string ToString()
        {
            return "Error Result : " + _Successful + " Message : " + _Exception.Message;
        }
    }

    public class MultiResult : IBaseResult
    {
        public MultiResult()
        {
            _results = new List<IBaseResult>();
        }
        private bool _Successful;
        private List<IBaseResult> _results;
        public bool Successful
        {
            get
            {
                bool overalResult = true;

                foreach (var res in _results)
                {
                    overalResult = overalResult && res.Successful;
                }

                return overalResult;
            }
            set => _Successful = value;
        }

        public List<IBaseResult> ResultContainer { get => _results; set => _results = value; }

        public override string ToString()
        {
            string str = "";

            foreach (var item in _results)
            {
                str += item.ToString() + " ";
            }

            return str;
        }
    }

    public class RegisterSuccessResult : IResult<byte[]>
    {
        private byte[] _PrivateKey;
        private byte[] _PublicKey;
        private bool _Successful;
        public bool Successful { get => _Successful; set => _Successful = value; }
        public byte[] ResultContainer { get => _PrivateKey; set => _PrivateKey = value; }
        public byte[] ResultPublicContainer { get => _PublicKey; set => _PublicKey = value; }
        public override string ToString()
        {
            return "RegisterSuccessResult Result : " + _Successful;
        }
    }
    public class SendFileMode : IResult<string>
    {
        private bool _Successful;
        private string? _Hash { get; set; }
        public string ResultContainer { get => this._Hash; set => this._Hash = value; }
        public bool Successful { get => _Successful; set => _Successful = value; }
    }
}
