using PurrNet;
using System.Collections;
using UnityEngine;

public class Pickup_Object : NetworkBehaviour
{
    public Transform T_modelHolder;
    public TrailRenderer TR_trailRenderer;
    public Collider C_collider;

    public AnimCurve A_movement;
    public AnimCurve A_height;

    private Resource.resourceClass _Resource;
    private Consumable.consumableClass _Consumable;
    private pickupEnum _PickUpType = pickupEnum.none;
    private enum pickupEnum { none, resource, consumable};
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Character"))
        {
            AgentLocation AL;
            if (!other.TryGetComponent<AgentLocation>(out AL))
                return;
            if (!AL.RM)
                return;
            if (!(AL.RM.controller is PlayerController))
                return;
            PlayerController PC = (PlayerController)AL.RM.controller;
            switch (_PickUpType)
            {
                case pickupEnum.resource:
                    PC.Pickup_Resource(_Resource);
                    break;
                case pickupEnum.consumable:
                    PC.Pickup_Consumable(_Consumable);
                    break;
                default:
                    break;
            }
            Collect();
        }
    }

    public void Setup(Resource_Type _type)
    {
        _PickUpType = pickupEnum.resource;
        _Resource = Resource.resourceClass.Create(_type);
        GameObject GO = Instantiate(_Resource.GetType().model, T_modelHolder);
        GO.transform.localPosition = Vector3.zero;
        StartCoroutine(Spawn());
    }
    public void Setup(Consumable_Type _type)
    {
        _PickUpType = pickupEnum.consumable;
        _Consumable = Consumable.consumableClass.Create(_type, 1);
        GameObject GO = Instantiate(_Consumable.Get_Item().model, T_modelHolder);
        GO.transform.localPosition = Vector3.zero;
        StartCoroutine(Spawn());
    }

    IEnumerator Spawn()
    {
        float timer = 0;
        Vector3 startPos = transform.localPosition;
        Vector3 movement = Random.insideUnitCircle;
            movement.z = movement.y;
            movement.y = 0;
        float height = 2f;
        Vector3 curPos;
        TR_trailRenderer.enabled = true;
        while (timer < 1)
        {
            curPos = startPos + Vector3.Lerp(Vector3.zero, movement, A_movement.Evaluate(timer));
            curPos.y += Mathf.Lerp(0, height, A_height.Evaluate(timer));
            transform.localPosition = curPos;
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        transform.localPosition = startPos + movement;
        yield return new WaitForSeconds(TR_trailRenderer.time);
        TR_trailRenderer.enabled = false;
    }
    [ObserversRpc]
    void Collect()
    {
        C_collider.enabled = false;
        StartCoroutine(Collect_Co());
    }
    IEnumerator Collect_Co()
    {
        float timer = 0;
        Vector3 startPos = transform.localPosition;
        Vector3 startAngle = transform.localEulerAngles;
        Vector3 endAngle = new Vector3(0,720,0);
        while (timer < 1)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, A_movement.Evaluate(timer));
            transform.localPosition = startPos + Vector3.Lerp(Vector3.zero, Vector3.one, A_movement.Evaluate(timer));
            transform.localEulerAngles = startAngle + Vector3.Lerp(Vector3.zero, endAngle, A_movement.Evaluate(timer));
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        Destroy(gameObject);
    }
}
