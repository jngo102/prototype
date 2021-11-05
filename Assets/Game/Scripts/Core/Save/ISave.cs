//
// Public API
//

public interface ISave
{
    SaveInt GetInt(string key, SaveMode mode = SaveMode.Bench, SaveScope scope = SaveScope.Local);
    SaveBool GetBool(string key, SaveMode mode = SaveMode.Bench, SaveScope scope = SaveScope.Local);
    SaveString GetString(string key, SaveMode mode = SaveMode.Bench, SaveScope scope = SaveScope.Local);
}

public interface ISaveSlot : ISave
{
    void Reset(SaveMode mode = SaveMode.Bench);
    void BeginScope(string key);
    void EndScope();
    void Flush();
}

public enum SaveMode
{
    Bench,
    Persistent
}

public enum SaveScope
{
    Local,
    Global
}

//
// SaveVar
//

public class SaveInt : SaveVar<int> { public override void Decode(string value) => Value = int.Parse(value); }
public class SaveString : SaveVar<string> { public override void Decode(string value) => Value = value; }
public class SaveBool : SaveVar<bool> { public override void Decode(string value) => Value = bool.Parse(value); }

public abstract class SaveVar<T> : SaveVar
{
    public T Value { get; protected set; }

    public void Write(T value) => Value = value;

    public override string Encode() => Value.ToString();
}

public abstract class SaveVar
{
    public abstract string Encode();

    public abstract void Decode(string value);
}