using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
    
public class CEvent : IEvent
{
    public CEvent()
    {

    }

    public CEvent(EventPredefined.MsgType ev_type)
    {
        m_ev_type = ev_type;
    }

    protected EventPredefined.MsgType m_ev_type;
    public override EventPredefined.MsgType GetEventType()
    {
        return m_ev_type;
    }
    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write((short)m_ev_type);
    }
    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        m_ev_type = (EventPredefined.MsgType) reader.ReadInt16();
    }
}

public class CEntityEvent : CEvent
{
    public CEntityEvent()
    {

    }
    public CEntityEvent(EventPredefined.MsgType ev_type)
        : base(ev_type)
    {

    }

    public CEntityEvent(int frame_index, int en_id, EventPredefined.MsgType ev_type)
        : base(ev_type)
    {
        FrameIndex = frame_index;
        EnId = en_id;
    }
    protected EventPredefined.EntityEventType m_en_et;
    public int FrameIndex { get; set; }
    public int EnId { get; set; }
    public EventPredefined.EntityEventType GetEntityEventType()
    {
        return m_en_et;
    }
    public bool IsLocal { get; set; }

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write((short)m_en_et);
        writer.Write(FrameIndex);
        writer.Write(EnId);
        writer.Write(IsLocal);
    }

    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        m_en_et = (EventPredefined.EntityEventType) reader.ReadInt16();
        FrameIndex = reader.ReadInt32();
        EnId = reader.ReadInt32();
        IsLocal = reader.ReadBoolean();
    }
}

public class CCreateEvent : CEntityEvent
{
    public CCreateEvent()
        : base(EventPredefined.MsgType.EMT_ENTITY_CREATE)
    {

    }

    public CCreateEvent(int frame_index, int en_id ,EntityPredefined.EntityType en_type, EntityPredefined.EntityCampType cmp, int spwan_pos_index)
        : base(frame_index, en_id, EventPredefined.MsgType.EMT_ENTITY_CREATE)
    {
        m_en_et = EventPredefined.EntityEventType.ET_CREATE;
        EnType = en_type;
        CampType = cmp;
        SpwanPosIndex = spwan_pos_index;
    }

    public EntityPredefined.EntityType EnType { get; set; }
    public EntityPredefined.EntityCampType CampType { get; set; }
    public int SpwanPosIndex { get; set; }

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write((short)EnType);
        writer.Write((short)CampType);
        writer.Write(SpwanPosIndex);
    }
    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        EnType = (EntityPredefined.EntityType) reader.ReadInt16();
        CampType = (EntityPredefined.EntityCampType) reader.ReadInt16();
        SpwanPosIndex = reader.ReadInt32();

    }

}

public class COpEvent : CEntityEvent
{
    public COpEvent()
        : base(EventPredefined.MsgType.EMT_ENTITY_OP)
    {

    }

    public COpEvent(int frame_index, int en_id, EntityPredefined.EntityOpType op_type)
        : base(frame_index, en_id, EventPredefined.MsgType.EMT_ENTITY_DESTROY)
    {
        m_en_et = EventPredefined.EntityEventType.ET_OP;
        OpType = op_type;
    }
    public EntityPredefined.EntityOpType OpType { get; set; }

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write((short) OpType);
    }
    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        OpType = (EntityPredefined.EntityOpType) reader.ReadInt16();

    }
}

public class CDestoryEvent : CEntityEvent
{
    public CDestoryEvent()
        : base(EventPredefined.MsgType.EMT_ENTITY_DESTROY)
    {

    }
    public CDestoryEvent(int frame_index, int en_id)
        : base(frame_index, en_id, EventPredefined.MsgType.EMT_ENTITY_OP)
    {
        m_en_et = EventPredefined.EntityEventType.ET_DESTROY;
    }
}

public class CEnterInGameEvent : CEvent
{
    public CEnterInGameEvent()
    {
        m_ev_type = EventPredefined.MsgType.EMT_ENTER_GAME;
    }
}

public class CEnterChangeSceneEvent : CEvent
{
    public CEnterChangeSceneEvent()
    {
        m_ev_type = EventPredefined.MsgType.EMT_ENTITY_CHANGE_SCENE;
    }
}
