using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;

namespace ExternalBanking
{

    public class CouponRepaymentDateCollection : List<CouponRepaymentScheduleItem>, IEnumerable<SqlDataRecord>
    {

        IEnumerator<SqlDataRecord> IEnumerable<SqlDataRecord>.GetEnumerator()
        {
            var sqlRow = new SqlDataRecord(
                  new SqlMetaData("coupon_repayment_date", SqlDbType.SmallDateTime));

            foreach (CouponRepaymentScheduleItem repaymentDateItem in this)
            {
                sqlRow.SetDateTime(0, repaymentDateItem.CouponRepaymentDate);
                yield return sqlRow;
            }
        }
    }

    public class CouponRepaymentScheduleItem
    {
        /// <summary>
        /// Արժեկտրոնների հաշվարկման օր
        /// </summary>
        public DateTime CouponRepaymentDate { get; set; }
    }

}
