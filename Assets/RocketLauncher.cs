using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject target;
    public GameObject rocket;
    public float rocketSpeed;
    public float fireCoolDown;
    float _firetimer;
    void Start()
    {
        _firetimer = Random.Range(-3,0);
    }


    // Update is called once per frame

    Vector3 targetVel = Vector3.zero;
    Vector3 prevTargetVel;
    public float rotationSpeed;
    void FixedUpdate()
    {
        if (GetComponent<Dmg>().hp < 0)
        {
            GetComponent<LineRenderer>().widthMultiplier = 0;
            fireCoolDown = 1000;
        }



        //aici incepe predictia
        float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
        //distanta-1 pentru ca atunci cand spawnam racheta, o spawnam un pic mai in fata lansatorului
        float aproxTravelTime = (distanceToTarget-1) / rocketSpeed;

        prevTargetVel = targetVel;
        targetVel = target.GetComponent<Rigidbody>().velocity;

        //iteratia initiala
        Vector3 predictedPoint = Predictor(aproxTravelTime);
        //iteratia 2 
        predictedPoint = Predictor((Vector3.Distance(predictedPoint, transform.position)-1) / rocketSpeed);
        predictedPoint = Predictor((Vector3.Distance(predictedPoint, transform.position)-1) / rocketSpeed);
       // predictedPoint = Predictor((Vector3.Distance(predictedPoint, transform.position)-1) / rocketSpeed);

        //rotim lansatorul catre player
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(predictedPoint - transform.position),
            Time.fixedDeltaTime*rotationSpeed);
        //transform.LookAt(predictedPoint);

        

        float distanceToPredicted = Vector3.Distance(transform.position, predictedPoint);


        //raza laser
        Ray raycast = new Ray();
        RaycastHit hit;
        raycast.origin = transform.position;
        raycast.direction = transform.forward;

        //raycast pt raza laser ca sa nu treaca prin chestii
        GetComponent<LineRenderer>().SetPosition(0, transform.position);
        if (!Physics.Raycast(raycast, out hit, distanceToPredicted))
        {
            GetComponent<LineRenderer>().SetPosition(1, transform.position +
                transform.forward * (distanceToPredicted-1));
        }
        else
        {
            GetComponent<LineRenderer>().SetPosition(1, transform.position +
                transform.forward * Vector3.Distance(transform.position, hit.point));
        }

        _firetimer += Time.fixedDeltaTime;


        //asteptam clear line of sight
        raycast.direction = predictedPoint - transform.position;
        bool Blocked = false;
        //Debug.DrawRay(raycast.origin, raycast.direction);
        if (_firetimer > fireCoolDown)
        {
            Blocked = Physics.Raycast(raycast, distanceToPredicted - 1);
            //facem asta pt ca dupa ce avem LoS sa mai asteptam un pic
            //a.i. racheta sa nu se loveasca de maarginea peretelui
            if (Blocked) _firetimer -= 0.2f;
        }

        //calculam eroare in unghi fata de unde ar trebuii sa tintim
        float AngleError = Vector3.Angle(transform.forward, 
            (predictedPoint - transform.position).normalized);

        //Debug.Log(360 / distanceToPredicted);
        Debug.DrawRay(transform.position + transform.forward, (predictedPoint - (transform.position + transform.forward)));


        //daca cooldown-ul a expirat, putem vedea playerul
        //si eroare de unghi e suficient de mica relativ la distanta

        if (_firetimer > fireCoolDown && !Blocked && AngleError<360/distanceToPredicted)
        {
            _firetimer = 0;

            //spawnam racheta un pic in fata lansatorului, orientata direct catre predictionPoint
            GameObject newRocket = Instantiate(rocket, transform.position + transform.forward,
                Quaternion.LookRotation((predictedPoint - (transform.position + transform.forward)).normalized));
            
            //setam timer-ul rachetei
            newRocket.GetComponent<Rocket>().explosionTimer =
                Vector3.Distance(predictedPoint, transform.position + transform.forward) / rocketSpeed;

            Destroy(newRocket, 10);
        }
    }

    Vector3 Predictor(float aproxTravelTime)
    {
        //PredictieA: adaugam viteza
        Vector3 predictedPoint = target.transform.position +
            targetVel * aproxTravelTime;



        //PredictieB: adaugam acceleratia
        Vector3 accel = (targetVel - prevTargetVel) / Time.fixedDeltaTime;
        //Debug.Log(accel);

        // Debug.Log((1 / 2) * -9.81f * aproxTravelTime * aproxTravelTime);
        //gravitatie
        //predictedPoint.y += aproxTravelTime * aproxTravelTime * -9.81f * 0.5f;

        //TODO: gasit o cale mai buna de a ignora schimbarile bruste de acceleratie
        accel = Vector3.ClampMagnitude(accel, 50);
        predictedPoint += accel * aproxTravelTime * aproxTravelTime * 0.5f;


        //PredictieC: calculam daca player-ul se va lovi de ceva
        Ray raycast = new Ray();
        RaycastHit hit;
        raycast.origin = target.GetComponent<Rigidbody>().position;

        

        Vector3 finalDeltaPos = predictedPoint - raycast.origin;
        raycast.direction = finalDeltaPos;


        //predictieD
        if (Physics.Raycast(raycast, out hit, finalDeltaPos.magnitude))
        {
            predictedPoint = hit.point + Vector3.ProjectOnPlane((predictedPoint - hit.point), hit.normal);

            if (RaycastBetween(hit.point+hit.normal, predictedPoint + hit.normal, out RaycastHit rh))
                predictedPoint = rh.point;
        }

        //TODO: raycast pt daca nu mai ramai pe suprafata de contact

        Debug.DrawRay(raycast.origin, finalDeltaPos, Color.green);
        return predictedPoint;
    }

    bool RaycastBetween(Vector3 origin, Vector3 dest, out RaycastHit rh)
    {
        return Physics.Raycast(origin, dest - origin, out rh, (dest - origin).magnitude);
    }

}
