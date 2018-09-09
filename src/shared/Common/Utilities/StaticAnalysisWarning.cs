namespace Common
{
    public class StaticAnalysisWarning
    {
        public string Warning { get; set; }
        public string Desciprtion { get; set; }
        public string Project { get; set; }
        public string File { get; set; }
        public int Line { get; set; }
        public string Output => Warning + " " + Desciprtion;
    }
}
