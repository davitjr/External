using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.DocFlowManagement
{
    public class MemoField
    {
        public int MemoId { get; set; }
        public int PositionId { get; set; }
        public string  ItemDescription { get; set; }
        public int InputType { get; set; }
        public int ControlType { get; set; }
        public string ControlName { get; set; }
        public string FieldValue { get; set; }
        public string FieldValueType { get; set; }
        public string ParametrName { get; set; }
        public string ParametrValue { get; set; }
    }
}
