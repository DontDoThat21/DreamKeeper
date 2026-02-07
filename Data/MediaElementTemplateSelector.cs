using DreamKeeper.Data.Models;

namespace DreamKeeper.Data
{
    /// <summary>
    /// Selects between ByteArrayMediaElementTemplate (recording present) 
    /// and BaseMediaElementTemplate (no recording) based on Dream.DreamRecording null-check.
    /// </summary>
    public class MediaElementTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ByteArrayMediaElementTemplate { get; set; }
        public DataTemplate? BaseMediaElementTemplate { get; set; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Dream dream && dream.DreamRecording != null && dream.DreamRecording.Length > 0)
                return ByteArrayMediaElementTemplate;

            return BaseMediaElementTemplate;
        }
    }
}
