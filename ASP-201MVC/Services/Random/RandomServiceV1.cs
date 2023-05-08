using System.Text;

namespace ASP_201MVC.Services.Random
{
    public class RandomServiceV1 : IRandomService
    {
        private readonly String safeChars = new String(
            Enumerable.Range(20, 107).Select(x => (char)x).ToArray());
        private readonly System.Random random = new();

        public String ConfirmCode(int length)
        {

            StringBuilder code = new StringBuilder(length);
            Guid testId = Guid.NewGuid();
            for (int i = 0; i < length; i++)
            {
                code.Append(testId.ToString()[random.Next(testId.ToString().Length)]);
            }
            return code.ToString();
        }
        public String RandomString(int length)
        {
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(safeChars.Length);
                sb.Append(safeChars[index]);
            }
            return sb.ToString();
        }

        public String RandomAvatarName(string fileName, int length)
        {
            String ext = Path.GetExtension(fileName);
            return ConfirmCode(length) + ext;
        }
    }
}
