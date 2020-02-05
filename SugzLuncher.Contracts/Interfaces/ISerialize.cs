using System.Xml.Linq;

namespace SugzLuncher.Contracts
{
    public interface ISerialize
    {
        public XElement Serialize();

        public void Deserialize(XElement xElement);

    }
}
