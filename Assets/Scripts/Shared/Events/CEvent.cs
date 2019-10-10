using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

public class CEntityEvent : CEvent
{
    public CEntityEvent(int frame_index, int en_id)
        : base(EventPredefined.MsgType.EMT_ENTITY_EVENT)
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
}

public class CCreateEvent : CEntityEvent
{
    public CCreateEvent(int frame_index, int en_id ,EntityPredefined.EntityType en_type, EntityPredefined.EntityCampType cmp, int spwan_pos_index)
        : base(frame_index, en_id)
    {
        m_en_et = EventPredefined.EntityEventType.ET_CREATE;
        EnType = en_type;
        CampType = cmp;
        SpwanPosIndex = spwan_pos_index;
    }

    public EntityPredefined.EntityType EnType { get; set; }
    public EntityPredefined.EntityCampType CampType { get; set; }
    public int SpwanPosIndex { get; set; }
}

public class COpEvent : CEntityEvent
{
    public COpEvent(int frame_index, int en_id, EntityPredefined.EntityOpType op_type)
        : base(frame_index, en_id)
    {
        m_en_et = EventPredefined.EntityEventType.ET_OP;
        OpType = op_type;
    }
    public EntityPredefined.EntityOpType OpType { get; set; }
}

public class CDestoryEvent : CEntityEvent
{
    public CDestoryEvent(int frame_index, int en_id)
        : base(frame_index, en_id)
    {
        m_en_et = EventPredefined.EntityEventType.ET_DESTROY;
    }
}
