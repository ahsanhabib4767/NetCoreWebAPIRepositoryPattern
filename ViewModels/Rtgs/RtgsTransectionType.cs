namespace RTGSWebApi.ViewModels.Rtgs
{
    public static class RtgsTransectionType
    {

        /// <summary>
        /// RtgsSSCT means RTGS Single Transaction
        /// </summary>
        public static string RtgsSSCT { get { return "RTGS_SSCT"; } }

        /// <summary>
        /// RtgsCSCT means RTGS Multiple Transaction
        /// </summary>
        public static string RtgsCSCT { get { return "RTGS_CSCT"; } }

        /// <summary>
        /// RtgsFICT means RTGS Financial Institute to Financial Institute (pacs.009) Transaction
        /// </summary>        
        public static string RtgsFICT { get { return "RTGS_FICT"; } }

        /// <summary>
        /// RtgsRETN means RTGS Return (pacs.004) Transaction
        /// </summary>
        public static string RtgsRETN { get { return "RTGS_RETN"; } }

    }
}
