using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyEvent: UnityEvent<DemoEnemy> {}

[RequireComponent(typeof(NavMeshAgent))]
public class DemoEnemy : MonoBehaviour {
    private NavMeshAgent m_Agent;
    private float m_Health = 100;
    private EnemyEvent m_OnDead = new EnemyEvent();
    
    public float Health {
        get {
            return m_Health;
        }
        set {
            m_Health = value;
        }
    }

    public EnemyEvent OnDead {
        get {
            return m_OnDead;
        }
    }

    public void Track(Vector3 enemyPosition) {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Agent.destination = enemyPosition;
    }

    void Update() {
        if (m_Health <= 0) {
            m_OnDead.Invoke(this);
            Destroy(gameObject);
        }
    }
}
