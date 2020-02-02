namespace articleservice
{

    public class Element
    {
        public enum Styles
        {
            None,
            Italic,
            Bold,
            Link,
            Quote
        }
        public Styles style;
        public string text;
        public string uri;
    }
}