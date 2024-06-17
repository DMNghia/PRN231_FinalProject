namespace FinalProject.Dto.Response
{
    public class BaseResponse<T>
    {
        public BaseResponse()
        {
        }

        public BaseResponse(int code, string message)
        {
            this.code = code;
            this.message = message;
        }

        public int code {  get; set; }
        public string message { get; set; }
        public T data { get; set; }
    }
}
