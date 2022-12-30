namespace FullNodeDataLogger.Model
{
    /// <summary>
    /// This interface is implemented in multiple result type below
    /// </summary>
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

    public class CreateEntityResult : IResult<int>
    {
        private int _EntityId;
        private bool _Successful;
        public bool Successful { get => _Successful; set => _Successful = value; }
        public int ResultContainer { get => _EntityId; set => _EntityId = value; }
        public override string ToString()
        {
            return "CreateEntityResult Result : " + _Successful + " Message : " + ResultContainer;
        }
    }
    public class UpdateEntityResult : IResult<int>
    {
        private int _EntityId;
        private bool _Successful;
        public bool Successful { get => _Successful; set => _Successful = value; }
        public int ResultContainer { get => _EntityId; set => _EntityId = value; }
        public override string ToString()
        {
            return "UpdateEntityResult Result : " + _Successful + " Message : " + ResultContainer;
        }
    }
    public class CreateLongEntityResult : IBaseResult
    {
        private long _EntityId;
        private bool _Successful;
        public bool Successful { get => _Successful; set => _Successful = value; }
        public long ResultContainer { get => _EntityId; set => _EntityId = value; }
        public override string ToString()
        {
            return "CreateLongEntityResult Result : " + _Successful + " Message : " + ResultContainer;
        }
    }
    public class CreateStringEntityResult : IBaseResult
    {
        private string _EntityId;
        private bool _Successful;
        public bool Successful { get => _Successful; set => _Successful = value; }
        public string ResultContainer { get => _EntityId; set => _EntityId = value; }
        public override string ToString()
        {
            return "CreateStringEntityResult Result : " + _Successful + " Message : " + ResultContainer;
        }
    }
}
