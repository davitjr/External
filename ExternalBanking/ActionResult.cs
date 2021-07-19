using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking
{
    /// <summary>
    /// Գործողության արդյունք
    /// </summary>
    public  class ActionResult
    {
        /// <summary>
        /// Գործուղության արդյունքի կոդ
        /// </summary>
        public ResultCode ResultCode { get; set; }
        /// <summary>
        /// Գործուղության արդյունքում առաջացած սխալներ
        /// </summary>
        public List<ActionError> Errors { get; set; }
        /// <summary>
        /// Գործողւութայն ենթարկվող օբեկտի ID
        /// </summary>
        public long Id { get; set; }

        public ActionResult()
        {
            Errors = new List<ActionError>();
        }
    }

    /// <summary>
    /// Գործողության սխալ
    /// </summary>
    public class ActionError
    {
        /// <summary>
        /// Սխալի կոդ
        /// </summary>
        public short Code { get; set; }

        /// <summary>
        /// Սխալի նկարագրություն
        /// </summary>
        public string Description { get; set; } 
        
        /// <summary>
        /// Սխալի պարամետրների ցուցակ
        /// </summary>
        public string[] Params {get;set;}

        public ActionError()
        {
        }
        public ActionError(short code)
        {
            Code = code;
        }
        public ActionError(short code,string[] paramsList)
        {
            Code = code;
            Params = paramsList;
        }
        public ActionError(string description)
        {
            Description = description;
        }
    }

    public class ContentResult<T>
    {
        /// <summary>
        /// Գործողության արդյունքի կոդ
        /// </summary>
        public ResultCode ResultCode { get; set; }
        /// <summary>
        /// Գործուղության արդյունքում առաջացած սխալներ
        /// </summary>
        public List<ActionError> Errors { get; set; }

        /// <summary>
        /// Գործողության արդյունքում առաջացած սխալների հաղորդագրությունները՝ 1 հաղորդագրության տեսքով
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Գործողության ենթարկվող օբեկտ
        /// </summary>
        public T Content{ get; set; }
    

        public ContentResult()
        {
            Errors = new List<ActionError>();
        }
    }
}
