using SilverCraft.CSCore.Tags.ID3.Frames;
using System;
using System.IO;

namespace SilverCraft.CSCore.Tags.ID3
{
    public class ID3v2QuickInfo
    {
        private ID3v2 _id3;

        private string TextFrameHelper(FrameID frameId) => _id3[frameId] is TextFrame x ? x.Text : string.Empty;
        public string Title => TextFrameHelper(FrameID.Title);

        public string Album => TextFrameHelper(FrameID.Album);
     

        public string Artist => TextFrameHelper(FrameID.OriginalArtist);
       

        public string LeadPerformers => TextFrameHelper(FrameID.LeadPerformers);
       

        public string Comments => TextFrameHelper(FrameID.Comments);
     

        public Stream? Image => _id3[FrameID.AttachedPicutre] is PictureFrame x ? x.Image : null;

        private static int? TryParseIntOrNull(string text) => int.TryParse(text, out var result) ? result : null;
        private int? NumericTextFrameHelper(FrameID frameId) => _id3[frameId] is NumericTextFrame n ? TryParseIntOrNull(n.Text) : null;
        
        public int? Year => NumericTextFrameHelper(FrameID.Year);

        //Thanks to AliveDevil
        public int? TrackNumber=> _id3[FrameID.TrackNumber] is MultiStringTextFrame n ? TryParseIntOrNull(n.Text) : null;
     

        public int? OriginalReleaseYear=>NumericTextFrameHelper(FrameID.OriginalReleaseYear);
       

        public ID3Genre? Genre
        {
            get
            {
                if (_id3[FrameID.ContentType] is not MultiStringTextFrame f)
                    return null;

                var str = f.Text;
                if (string.IsNullOrEmpty(str) || !str.StartsWith('(') || str.Length < 3)
                {
                    try
                    {
                        return Enum.Parse<ID3Genre>(str);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                char c;
                var i = 1;
                var sr = string.Empty;
                do
                {
                    c = str[i++];
                    if (char.IsNumber(c))
                        sr += c;
                } while (i < str.Length && char.IsNumber(c));

                if (int.TryParse(sr, out var res))
                {
                    return (ID3Genre)res;
                }
                return null;
            }
        }

        public ID3v2QuickInfo(ID3v2 id3)
        {
            ArgumentNullException.ThrowIfNull(id3);
            _id3 = id3;
        }
    }
}