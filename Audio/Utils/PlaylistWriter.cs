using System.Xml;

namespace Audio.Utils;
public class PlaylistWriter : IDisposable
{
    private const string _xmlns = "http://xspf.org/ns/0/";
    private const string _fileName = "playlist.xspf";

    private readonly XmlWriter _writer;
    private bool disposedValue;

    public PlaylistWriter(string directory) : this(File.OpenWrite(Path.Combine(directory, _fileName))) { }
    public PlaylistWriter(Stream stream)
    {
        _writer = XmlWriter.Create(stream, new() { Indent = true });
        _writer.WriteStartElement("playlist", _xmlns);
        _writer.WriteAttributeString("version", "1");
        _writer.WriteStartElement("trackList");
    }

    ~PlaylistWriter()
    {
        Dispose(false);
    }

    public void WriteTrack(string location, string title = "", string comments = "")
    {
        _writer.WriteStartElement("track");
        _writer.WriteElementString("location", location);
        if (!string.IsNullOrEmpty(title)) _writer.WriteElementString("title", title);
        if (!string.IsNullOrEmpty(comments)) _writer.WriteElementString("annotation", comments);
        _writer.WriteEndElement();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _writer.WriteEndElement();
                _writer.WriteEndElement();
                _writer.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
