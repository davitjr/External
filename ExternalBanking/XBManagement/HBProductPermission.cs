﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalBanking.XBManagement
{
    public class HBProductPermission
    {
        /// <summary>
        /// Պրոդուկտի տեսակը
        /// </summary>
        public HBProductPermissionType ProductType { get; set; }
        /// <summary>
        /// Հաշվեհամար
        /// </summary>
        public string ProductAccountNumber { get; set; }
        /// <summary>
        /// Պրոդուկտի AppID 
        /// </summary>
        public ulong ProductAppID { get; set; }
        /// <summary>
        /// Ակտիվության կարգավիճակ
        /// </summary>
        public bool IsActive { get; set; }



    }
}
