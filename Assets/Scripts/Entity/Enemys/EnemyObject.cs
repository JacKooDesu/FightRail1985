﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JacDev.UI.GameScene;
using JacDev.Mod;

namespace JacDev.Entity
{
    public class EnemyObject : EntityObject
    {
        [Header("等級")]
        // 需於Spawner 時設定完成，順便把攻擊係數調整好
        // 當前100等為最高
        public int level;
        [Header("攻擊設定")]
        public ProjectileSpawner launcher;
        // public Enemy enemy;
        // public static TrainLine target;
        public EntityObject target;
        [SerializeField] Animator ani;
        EntityObject attackTarget;
        Collider taretCol;

        bool hasAttackOnce = false; // 是否攻擊過一次，避免沒攻擊就消失
        float notAttack = 0;
        bool attacking = false; // 之後應以狀態寫成寫成Enum
        bool hasAttack = false; // 是否進行攻擊(投射器or進戰傷害觸發)
        float attackTimeCounter = 0f;  // 攻擊時間(每次攻擊所花時間)
        public float changeTargetTime = 3f;

        [Header("UI Settings")]
        public Slider healthBar;


        private void Awake()
        {
            if (ani == null)
                ani = GetComponent<Animator>();

            var setting = ((Enemy)entitySetting);
            maxHealth = setting.health;
            health = maxHealth;

            launcher.owner = this;

            // 還需要被定義傷害公式
            damage = setting.damage * (1);
        }

        private void LateUpdate()
        {
            TestMove();
            UpdateUI();

            if (health <= 0)
                OnDead();
        }

        public void TestMove()
        {
            Enemy setting = (Enemy)entitySetting;
            // if (target == null)
            //     return;
            if (FindObjectOfType<TrainLine>() == null)
                return;

            if (taretCol == null || notAttack >= changeTargetTime)
            {
                ChangeAttackTarget();
                notAttack = 0;
            }

            if (attacking)
            {
                if (attackTimeCounter <= setting.attackTime)
                {
                    attackTimeCounter += Time.deltaTime;
                    if (attackTimeCounter >= setting.attackTimeOffset && !hasAttack)
                    {
                        launcher.Launch(taretCol.ClosestPoint(launcher.transform.position));
                        hasAttack = true;
                    }
                    return;
                }
                attacking = false;
                attackTimeCounter = 0f;
            }


            notAttack += Time.deltaTime;
            // need added attack range in future
            if ((taretCol.ClosestPoint(transform.position) - GetComponent<Collider>().ClosestPoint(target.transform.position)).magnitude >= setting.attackRange)
            {
                if (!ani.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    transform.Translate(setting.movementSpeed * Vector3.forward * Time.deltaTime);
                    transform.LookAt(taretCol.ClosestPoint(transform.position));
                }

            }
            else
            {
                // print("can attack");
                transform.LookAt(taretCol.ClosestPoint(transform.position));
                if (!ani.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    ani.SetTrigger("Attack");
                    attacking = true;
                    notAttack = 0;
                    taretCol = null;
                    hasAttack = false;
                    // hasAttackOnce = true;
                }

            }
            // print(ani.GetCurrentAnimatorClipInfo(0)[0].clip);

            Ray r = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(r, out hit, 5f, 1 << 8))
            {
                // print(this.name);
                transform.Rotate(Vector3.up * 90);
            }
        }

        // 後續須改寫成可攻擊己方單位
        public void ChangeAttackTarget()
        {
            TrainLine trainLine = FindObjectOfType<TrainLine>();

            // Near Train
            float targetDistance = Vector3.Distance(trainLine.trains[0].transform.position, transform.position);
            int index = 0;
            for (int i = 0; i < trainLine.trains.Length; ++i)
            {
                if (targetDistance > Vector3.Distance(trainLine.trains[i].transform.position, transform.position))
                {
                    targetDistance = Vector3.Distance(trainLine.trains[i].transform.position, transform.position);
                    index = i;
                }
            }

            target = trainLine.trains[index];
            taretCol = trainLine.trains[index].GetComponent<Collider>();

            // if (targetDistance > ((Enemy)entitySetting).maxDet && hasAttackOnce)
            // {
            //     GameHandler.Singleton.entities.Remove(this);
            //     Destroy(gameObject);
            // }
            // taretCol = target.trains[Random.Range(0, target.trains.Length)].GetComponent<Collider>();
        }

        public void UpdateUI()
        {
            healthBar.transform.parent.LookAt(Camera.main.transform, Vector3.down);
            healthBar.value = Mathf.Clamp01(health / maxHealth);
        }

        public override bool GameUpdate()
        {
            return true;
        }

        // public override void Init(EntitySetting setting)
        public override void Init()
        {
            // base.Init(setting);
            level = Random.Range(0, 101); // for test
        }

        public override void GetDamage(float damage)
        {
            base.GetDamage(damage);
            if (GameSceneUIHandler.Singleton.damagePanel != null)
                GameSceneUIHandler.Singleton.damagePanel.DisplayDamage(this, damage);
        }

        public void OnDead()
        {
            GameHandler.Singleton.credit += (entitySetting as Enemy).dropMoney;
            Drop();
            Destroy(gameObject);
        }

        void Drop()
        {
            var dropTable = (entitySetting as Enemy).dropTable;
            if (dropTable == null)
                return;

            var drop = dropTable.CalDrop(this);

            if (drop is Item.DropTable.ItemDropSetting)
            {
                print((drop as Item.DropTable.ItemDropSetting).item.itemName);
            }
            else if (drop is Item.DropTable.ModDropSetting)
            {
                var mod = drop as Item.DropTable.ModDropSetting;
                var modObject = Instantiate(SettingManager.Singleton.ModList.modPrefab).GetComponent<ModObject>();
                modObject.transform.position = transform.position;
                modObject.BuildMod(mod.GetLevelDrop(level));
                print((drop as Item.DropTable.ModDropSetting).mod.modName);
            }
        }
    }
}
