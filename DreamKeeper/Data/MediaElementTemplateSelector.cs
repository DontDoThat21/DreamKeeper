using DreamKeeper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.Data
{
    public class MediaElementTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BaseMediaElementTemplate { get; set; }
        public DataTemplate ByteArrayMediaElementTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Dream dream && dream.DreamRecording != null)
            {
                return ByteArrayMediaElementTemplate;
            }
            else
            {
                return BaseMediaElementTemplate;
            }
        }
    }
}
