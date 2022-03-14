using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKWalker : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform leftLeg;
    public float animSpeed;
    public Transform leftdefaultPos;
    Vector3 leftLegPos;
    Vector3 leftTargetPos;
    public float safeStrideDistance;
    public float legLift;
    void Start()
    {
        leftLegPos = leftLeg.position;
        leftTargetPos = leftLegPos;
    }

    // Update is called once per frame
    void Update()
    {

        //leftdefaultPos.position este locul unde sta varful piciorului "default"
        //piciorul in general va sta "lipit" de leftTargetPos
        //daca varful piciorului este prea departe de locul unde sta default, inseamna ca ar trebuii sa il mutam
        if (Vector3.Distance(leftTargetPos, leftdefaultPos.position) > safeStrideDistance)
        {
            //daca piciorul a ramas in spate, ar trebuii sa il mutam in fata la o distanta egala cu "raza" pasului (safeStrideDistance)
            Vector3 newPos = leftdefaultPos.position + (leftdefaultPos.position - leftTargetPos).normalized * safeStrideDistance;
            //raycast spre sol ca sa vedem unde exact tintim sa punem varul piciorului (asta ii permite si sa urce peste obstacole)
            if (Physics.Raycast(newPos + Vector3.up, Vector3.down, out RaycastHit hit))
                leftTargetPos = hit.point;
        }
        //mutam varful piciorului catre "tinta"
        MoveLegToward(ref leftLegPos, leftTargetPos);
        //ridicam piciorul
        leftLeg.position = leftLegPos + Vector3.up * liftValue(leftLegPos, leftTargetPos);
    }

    void MoveLegToward(ref Vector3 LegPos, Vector3 target)
    {
        //mutam pozitia piciorului catre locul unde tintim sa punem urmatorul pas
        LegPos = Vector3.MoveTowards(LegPos, target, Time.deltaTime * animSpeed);
    }

    float liftValue(Vector3 leg, Vector3 target)
    {
        //teoretic asta o sa returneze 1 atunci cand distanta intre leg si target e egala cu sSD
        //pt ca gen... daca piciorul se muta atunci cand e la distanta mai mare de sSD de centru, si se muta cu sSD mai in fata
        //in principiu va avea de parcurs distanta 2*sSD, iar la mijloc ar trebuii sa fie cel mai sus
        return (1 - Mathf.Abs(safeStrideDistance - Vector3.Distance(leg, target)) / safeStrideDistance) * legLift;
    }
}
