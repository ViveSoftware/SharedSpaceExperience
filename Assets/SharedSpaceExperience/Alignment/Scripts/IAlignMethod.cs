namespace SharedSpaceExperience
{
    public interface IAlignMethod
    {
        public bool ExportAlignData(out AlignData alignData);
        public bool ImportAlignData(AlignData alignData);
    }
}