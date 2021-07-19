using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    public class PhoneBankingContractQuestionAnswer
    {
        /// <summary>
        /// Պատասխանի ունիկալ համար (Id)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Հարցի համար 
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Հարցի նկարագրություն 
        /// </summary>
        public string QuestionDescription { get; set; }

        /// <summary>
        /// Պատասխան
        /// </summary>
        public string Answer { get; set; }
    }

    public class QuestionAnswerCollection : List<PhoneBankingContractQuestionAnswer>, IEnumerable<SqlDataRecord>
    {
        IEnumerator<SqlDataRecord> IEnumerable<SqlDataRecord>.GetEnumerator()
        {
            var sqlRow = new SqlDataRecord(
                  new SqlMetaData("question_id", SqlDbType.Int),
                  new SqlMetaData("answer", SqlDbType.NVarChar, 1500));

            foreach (PhoneBankingContractQuestionAnswer questionAnswer in this)
            {
                sqlRow.SetInt32(0, questionAnswer.QuestionId);
                sqlRow.SetString(1, questionAnswer.Answer);

                yield return sqlRow;
            }
        }
    }
}
