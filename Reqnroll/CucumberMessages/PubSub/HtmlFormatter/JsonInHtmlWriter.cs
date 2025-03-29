using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cucumber.HtmlFormatter;

public class JsonInHtmlWriter : StreamWriter
{
    private readonly int BUFFER_SIZE = 1024;
    private StreamWriter Writer;
    private char[] escapeBuffer;

    public JsonInHtmlWriter(StreamWriter writer) : base(writer.BaseStream)
    {
        Writer = writer;
        escapeBuffer = new char[BUFFER_SIZE]; // Initialize escapeBuffer
    }

    public override void Write(string value)
    {
        Write(value.ToCharArray(), 0, value.Length);
    }

    public override void Write(char[] value)
    {
        Write(value, 0, value.GetLength(0));
    }

    public override void Write(char[] source, int offset, int length)
    {
        if (offset + length > source.GetLength(0))
            throw new ArgumentException("Cannot read past the end of the input source char array.");

        char[] destination = PrepareBuffer();
        int flushAt = BUFFER_SIZE - 2;
        int written = 0;
        for (int i = offset; i < offset + length; i++)
        {
            char c = source[i];

            // Flush buffer if (nearly) full
            if (written >= flushAt)
            {
                Writer.Write(destination, 0, written);
                written = 0;
            }

            // Write with escapes
            if (c == '/')
            {
                destination[written++] = '\\';
            }
            destination[written++] = c;
        }
        // Flush any remaining
        if (written > 0)
        {
            Writer.Write(destination, 0, written);
        }
    }

    private char[] PrepareBuffer()
    {
        // Reuse the same buffer, avoids repeated array allocation
        escapeBuffer ??= new char[BUFFER_SIZE];
        return escapeBuffer;
    }

    public override void Flush()
    {
        Writer.Flush();
    }

    public override void Close()
    {
        Writer.Close();
    }
}
