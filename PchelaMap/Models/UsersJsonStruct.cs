using System;
using System.Collections.Generic;
namespace PchelaMap.Models
{
    public class UsersJsonStruct
    {
        public string type { get; set; }
        public IList<MapFeature> features { get; set; }
    }
    public class MapFeature
    {
        public string type { get; set; }
        public string id { get; set; }
        public MapFeatureGeometry geometry { get; set; }
        public MapFeatureProperties properties { get; set; }
        public MapFeatureOptions options { get; set; }
    }
    public class MapFeatureGeometry
    {
        public string type { get; set; }
        public List<string> coordinates { get; set; }
    }
    public class MapFeatureProperties
    {
        public string hintContent { get; set; }
    }
    public class MapFeatureOptions
    {
        public string iconLayout { get; set; }
        public string iconImageHref { get; set; }
        public string fillImageHref { get; set; }
        public int[] iconImageSize { get; set; }
        public int[] iconImageOffset { get; set; }
        public MapFeatureIconShape iconShape { get; set; }
    }
    public class MapFeatureIconShape
    {
        public string type { get; set; }
        public int[] coordinates { get; set; }
        public int radius { get; set; }
    }
}
