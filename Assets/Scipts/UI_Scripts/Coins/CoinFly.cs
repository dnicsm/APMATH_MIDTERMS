using UnityEngine;
using System.Collections;

public class CoinFly : MonoBehaviour
{
    public float scatterDistance = 1.5f;
    public float scatterTime = 0.25f;
    public float waitBeforeCollect = 0.3f;
    public float flySpeed = 2500f;
    public int coinValue = 1;
    private Transform target;

  IEnumerator Start()
    {
        target = CoinManager.Instance.coinTarget;

        if (target == null)
        {
            yield break;
        }

        Vector3 randomDirection = Random.insideUnitCircle.normalized;

        Vector3 scatterTarget =
            transform.position +
            randomDirection *
            Random.Range(
                scatterDistance * 0.5f,
                scatterDistance
            );

        Vector3 startPos = transform.position;

        float timer = 0f;

        while (timer < scatterTime)
        {
            timer += Time.deltaTime;

            float t = timer / scatterTime;

            transform.position =
                Vector3.Lerp(
                    startPos,
                    scatterTarget,
                    t
                );

            yield return null;
        }

        yield return new WaitForSeconds(waitBeforeCollect);


        yield return new WaitForSeconds(
            Random.Range(0f, 0.4f)
        );


        while (true)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    flySpeed * Time.deltaTime
                );

            if (Vector3.Distance(
                transform.position,
                target.position) < 0.1f)
            {
                CoinManager.Instance.AddCoins(coinValue);

                Destroy(gameObject);

                yield break;
            }

            yield return null;
        }
    }
}