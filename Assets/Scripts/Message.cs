public class Message
{
    public string Text;
    public float Priority;
    public float Time;

    public Message(string text, float priority, float time)
    {
        Text = text;
        Priority = priority;
        Time = time;
    }
}