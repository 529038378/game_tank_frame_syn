using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

    public COpEvent(int frame_index, int en_id, EntityPredefined.EntityOpType op_type, EntityPredefined.EntityExtOpType op_ext_type)
        : base(frame_index, en_id, EventPredefined.MsgType.EMT_ENTITY_OP)
    {
        m_en_et = EventPredefined.EntityEventType.ET_OP;
        OpType = op_type;
        OpExtType = op_ext_type;
    }
    public EntityPredefined.EntityOpType OpType { get; set; }
    public EntityPredefined.EntityExtOpType OpExtType { get; set; }
    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        writer.Write((short) OpType);
        writer.Write((short) OpExtType);
    }
    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);
        OpType = (EntityPredefined.EntityOpType) reader.ReadInt16();
        OpExtType = (EntityPredefined.EntityExtOpType) reader.ReadInt16();
    }
}

public class CDestoryEvent : CEntityEvent
{
    public CDestoryEvent()
        : base(EventPredefined.MsgType.EMT_ENTITY_DESTROY)
    {

    }
    public CDestoryEvent(int frame_index, int en_id)
        : base(frame_index, en_id, EventPredefined.MsgType.EMT_ENTITY_DESTROY)
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

public class CClientReadyEvent : CEvent
{
    public CClientReadyEvent()
    {
        m_ev_type = EventPredefined.MsgType.EMT_CLIENT_READY;
    }
}

public class CClientLeaveInGameEvent : CEvent
{
    public CClientLeaveInGameEvent()
    {
        m_ev_type = EventPredefined.MsgType.EMT_CLIENT_LEAVE_INGAME;
    }
}

public class CSynOpEvent : CEntityEvent
{
    public CSynOpEvent()
        : base(EventPredefined.MsgType.EMT_SYN_ENTITY_OPS)
    {
        RecordEnEvs = new List<IEvent>();
    }

    //key : en_id, value : event
    public List<IEvent> RecordEnEvs { get; set; }
    public override void Deserialize(NetworkReader reader)
    {
        base.Deserialize(reader);

        byte[] bytes = reader.ReadBytes((int)(reader.Length - reader.Position));
        NetworkReader imp_reader = new NetworkReader(bytes);
        //分批读取
        while (0 != bytes.Length)
        {
            EventPredefined.MsgType ev_type =(EventPredefined.MsgType) bytes[0];
            imp_reader = new NetworkReader(bytes);
            IEvent ev = null;
            switch (ev_type)
            {
                case EventPredefined.MsgType.EMT_ENTITY_CREATE:
                ev = new CCreateEvent();
                break;
                case EventPredefined.MsgType.EMT_ENTITY_DESTROY:
                ev = new CDestoryEvent();
                break;
                case EventPredefined.MsgType.EMT_ENTITY_OP:
                ev = new COpEvent();
                break;
            }
            ev.Deserialize(imp_reader);
            //debug代码
            COpEvent oe = ev as COpEvent;
            bytes = imp_reader.ReadBytes((int)(imp_reader.Length - imp_reader.Position));
            imp_reader = new NetworkReader(bytes);
            RecordEnEvs.Add(ev);
        }
    }

    public override void Serialize(NetworkWriter writer)
    {
        base.Serialize(writer);
        foreach (var ev in RecordEnEvs)
        {
            ev.Serialize(writer);
        }
    }
}