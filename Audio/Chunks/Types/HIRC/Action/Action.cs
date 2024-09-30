namespace Audio.Chunks.Types.HIRC;
public record Action : HIRCObject
{
    public ActionScope Scope { get; set; }
    public ActionType Type { get; set; }
    public FNVID<uint> GameObject { get; set; }
    public bool GameObjectIsBus { get; set; }
    public ActionProperty[] Properties { get; set; } = [];
    public ActionModifier[] Modifiers { get; set; } = [];
    public IActionParameter Parameters { get; set; }

    public Action(HeaderInfo header) : base(header)
    {
        GameObject = 0;
        Parameters = new DefaultActionParameter();
    }

    public override void Read(BankReader reader)
    {
        base.Read(reader);

        Scope = (ActionScope)reader.ReadByte();
        Type = (ActionType)reader.ReadByte();
        GameObject = reader.ReadUInt32();
        GameObjectIsBus = reader.ReadByte() != 0;

        int propertyCount = reader.ReadByte();
        Properties = new ActionProperty[propertyCount];
        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i] = new();
        }

        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i].ID = (PropertyID)reader.ReadByte();
        }

        for (int i = 0; i < propertyCount; i++)
        {
            Properties[i].Value = reader.ReadUInt32();
        }

        int modifierCount = reader.ReadByte();
        Modifiers = new ActionModifier[modifierCount];
        for (int i = 0; i < modifierCount; i++)
        {
            Modifiers[i] = new();
        }

        for (int i = 0; i < modifierCount; i++)
        {
            Modifiers[i].ID = (PropertyID)reader.ReadByte();
        }

        for (int i = 0; i < modifierCount; i++)
        {
            Modifiers[i].Min = reader.ReadUInt32();
            Modifiers[i].Max = reader.ReadUInt32();
        }

        Parameters = Type switch
        {
            ActionType.Stop => new StopActionParameter(),
            ActionType.Pause => new PauseActionParameter(),
            ActionType.Resume => new ResumeActionParameter(),
            ActionType.Play or ActionType.PlayAndContinue => new PlayActionParameter(),
            ActionType.SetState => new SetStateActionParameter(),
            ActionType.SetGameParameter or ActionType.ResetGameParameter => new SetGameParameterActionParameter(),
            ActionType.SetSwitch => new SetSwitchActionParameter(),
            ActionType.SetBypassEffect or ActionType.ResetBypassEffect => new BypassEffectActionParameter(),
            ActionType.Seek => new SeekActionParameter(),
            ActionType.Mute or ActionType.Unmute or ActionType.ResetPlaylist => new ValueExceptionActionParameter(),
            ActionType.SetPitch or ActionType.ResetPitch
            or ActionType.SetVolume or ActionType.ResetVolume
            or ActionType.SetBusVolume or ActionType.ResetBusVolume
            or ActionType.SetLowPassFilter or ActionType.ResetLowPassFilter
            or ActionType.SetHighPassFilter or ActionType.ResetHighPassFilter => new PropertyActionParameter(),
            ActionType.UseState or ActionType.UnuseState
            or ActionType.StopEvent or ActionType.PauseEvent or ActionType.ResumeEvent
            or ActionType.Duck or ActionType.Break or ActionType.Trigger or ActionType.Release
            or ActionType.PlayEvent => new DefaultActionParameter(),
            _ => new StopActionParameter()
        };

        Parameters.Read(reader);
    }
}
