namespace CmsApi.DTOs
{
    public class ReferenceData
    {
        public ReferenceData(dynamic value, string label)
        {
            Value = value;
            Label = label;
        }
        public dynamic Value { get; private set; }
        public string Label { get; private set; }
    }
}
