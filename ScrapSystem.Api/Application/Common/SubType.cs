namespace ScrapSystem.Api.Application.Common
{
    public static class Commons
    {
        public static string ROH = "ROH";
        public static string HALB = "HALB";

        public static string[] appendix1 = new string[] { "551", "201" };
        public static string[] appendix2 = new string[] { "555", "556", "559", "560"};

    }
    public enum Appendix
    {
        APPENDIX1 = 1,
        APPENDIX2 = 2,
        APPENDIX3 = 3,
    }
}
