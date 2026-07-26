using System.Collections.Generic;

namespace RePKG.Core.Texture
{
    public interface ITexFrameInfoContainer
    {
        string Magic { get; set; }
        IList<ITexFrameInfo> Frames { get; }
        int Width { get; set; }
        int Height { get; set; }
    }
}