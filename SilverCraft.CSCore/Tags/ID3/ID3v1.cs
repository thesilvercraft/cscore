
namespace SilverCraft.CSCore.Tags.ID3;

//http://id3.org/ID3v1
/// <summary>
/// Represents an ID3v1 tag structure used in audio files.
/// </summary>
public class ID3v1
{
    public static ID3v1 FromFile(string filename)
    {
        using (var stream = File.OpenRead(filename))
        {
            return FromStream(stream);
        }
    }

    public static ID3v1 FromStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
            throw new ArgumentException("stream is not readable");

        long? pos = null;
        if (stream.CanSeek)
        {
            pos = stream.Position;
            stream.Position = stream.Length - 128;
        }

        ID3v1 tag = null;
        var reader = new BinaryReader(stream);
        if (reader.ReadByte() == 0x54 && reader.ReadByte() == 0x41 && reader.ReadByte() == 0x47)
        {
            tag = new ID3v1(stream);
        }

        if (pos != null)
            stream.Position = pos.Value;

        return tag;
    }

    public static ID3v1 CreateEmpty()
    {
        return new ID3v1();
    }

    /// <summary>
    /// Gets or sets the title of the audio file.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the name of the artist associated with the audio file.
    /// </summary>
    public string Artist { get; set; }

    /// <summary>
    /// Gets or sets the album name.
    /// </summary>
    public string Album { get; set; }

    /// <summary>
    /// Gets or sets the year of the audio file.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the comment text associated with the ID3v1 tag.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Represents the genre information for an ID3v1 tag.
    /// </summary>
    public ID3Genre Genre { get; set; }

    private ID3v1() { }

    private ID3v1(Stream stream)
    {
        var reader = new BinaryReader(stream);
        Title = new string(reader.ReadChars(30)).Replace("\0", string.Empty).TrimEnd();
        Artist = new string(reader.ReadChars(30)).Replace("\0", string.Empty).TrimEnd();
        Album = new string(reader.ReadChars(30)).Replace("\0", string.Empty).TrimEnd();
        int year;
        var parseResult = int.TryParse(new string(reader.ReadChars(4)), out year);
        if (parseResult)
            Year = year;
        else
            Year = null;
        Comment = new string(reader.ReadChars(30)).Replace("\0", string.Empty).TrimEnd();
        Genre = (ID3Genre)reader.ReadByte();
    }

    public void SaveToStream(Stream stream)
    {
        var writer = new BinaryWriter(stream);
        var title = Title.Length > 30 ? Title.Substring(0, 30) : Title;
        var artist = Artist.Length > 30 ? Title.Substring(0, 30) : Artist;
        var album = Album.Length > 30 ? Album.Substring(0, 30) : Album;
        var year = Year.HasValue ? Year.Value : 0;
        var comment = Comment.Length > 30 ? Comment.Substring(0, 30) : Comment;
        var genre = (byte)Genre;

        writer.Write(title);
        writer.Write(artist);
        writer.Write(album);
        writer.Write(year);
        writer.Write(comment);
        writer.Write(genre);
        writer.Flush();
    }
}