using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NePishiteSetiNa100ballov
{
    public class Question
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Dop_param { get; set; }
        public decimal SumK { get; set; }
        public List<int> Answer_code { get; set; } = new List<int>();
        public List<string> Answer { get; set; } = new List<string>();
        public List<decimal> K { get; set; } = new List<decimal>();
        public Question(object code, object answer_code, object k, object type, object dop_param, object answer)
        {
            Code = (int)code;
            SumK = (decimal)k;
            Answer_code.Add((int)answer_code);
            Answer.Add(answer.ToString());
            K.Add((decimal)k);
            Type = type.ToString();
            Dop_param = dop_param.ToString();
        }
        public void AddAnswer(object answer_code, object answer, object k = null)
        {
            if (k == null)
                K.Add(0);
            else
            {
                SumK += (decimal)k;
                K.Add((decimal)k);
            }
            Answer_code.Add((int)answer_code);
            Answer.Add(answer.ToString());
        }

        public int GetRightCount()
        {
            int counter = 0;
            for (int i = 0; i < K.Count; i++)
            {
                if (K[i] > 0) counter++;
            }
            return counter;
        }
    }
}
