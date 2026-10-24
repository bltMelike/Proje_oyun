// LLM kararını oyun durumuna çeviren her sınıfın sözleşmesi.
public interface INpcActionHandler
{
    void Handle(string action);
}