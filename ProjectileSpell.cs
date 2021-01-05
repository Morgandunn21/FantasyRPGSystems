using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Spells/Projectiles")]
public class ProjectileSpell : PlayerSpell
{
    //How fast a projectile spell moves
    public int Speed;

    protected override void ApplySpell(Transform attachTrans, TargetableObject target)
    {
        //Allow the particaly to wait to be cast until the animation has begun
        Timing.RunCoroutine(_Attack(attachTrans, target), Segment.FixedUpdate);
    }

    private IEnumerator<float> _Attack(Transform attachTrans, TargetableObject target)
    {
        GameObject projectile = Instantiate(ParticlePrefab, attachTrans.position, Quaternion.identity) as GameObject;
        var mps = projectile.GetComponent<MagicProjectileScript>();

        //Set up the targeting in case it hits an object now
        mps.TargetHit += (hitObject) =>
        {
            //Only apply the damage if you are the player
            if (player != null && playerControl != null)
            {
                var dmgMin = DmgMin + player.GetBalanceAttackAssist();
                dmgMin = System.Math.Min(dmgMin, DmgMax);
                int damageBuff = player.GetMagicDamageBuff();

                uint damage = (uint)(System.Math.Max(0, Random.Range(dmgMin, DmgMax + 1) + damageBuff));

                //Take Damage when the particle has hit the enemy
                DealDamage(hitObject, damage);

                if (statusEffect != null)
                {
                    statusEffect.ApplyEffect(hitObject);
                }
            }
        };

        yield return Timing.WaitForSeconds(CastDuration);

        //Show Attack Particle
        CreateParticle(projectile, attachTrans, target.mHitTransform);
    }

    private void CreateParticle(GameObject projectile, Transform start, Transform end)
    {
        var direction = end.position - start.position;
        Ray ray = new Ray(start.position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            Debug.DrawRay(start.position, direction, Color.green, hit.distance);

            if (projectile == null)
            {
                Debug.Log("Projectile already destroyed");
                return;
            }

            projectile.transform.LookAt(hit.point);
            projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * Speed);
            var mps = projectile.GetComponent<MagicProjectileScript>();
            mps.impactNormal = hit.normal;

            Timing.RunCoroutine(_Track(projectile, end), Segment.FixedUpdate);
        }
    }

    private IEnumerator<float> _Track(GameObject projectile, Transform target)
    {
        while (onCooldown)
        {
            yield return Timing.WaitForSeconds(Time.fixedDeltaTime);

            HomeInOnTarget(projectile, target);
        }
    }

    private void HomeInOnTarget(GameObject projectile, Transform target)
    {
        if (projectile == null || target == null)
        {
            return;
        }

        var direction = target.position - projectile.transform.position;
        Ray ray = new Ray(projectile.transform.position, direction);

        Debug.DrawRay(projectile.transform.position, direction, Color.green, 10f);

        if (Physics.Raycast(ray, out RaycastHit hit, Range))
        {
            projectile.transform.LookAt(hit.point);
            projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * Speed);
            projectile.GetComponent<MagicProjectileScript>().impactNormal = hit.normal;
        }
    }

    public override bool RequiresTarget()
    {
        return true;
    }

    protected override float CalculateCooldown()
    {
        return CastDuration + Cooldown;
    }
}
