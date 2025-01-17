﻿using DG.Tweening;
using UnityEngine;

namespace ItsBaptiste.Toolbox.Procedural.Tentacle.Scripts {
   [DisallowMultipleComponent]
    public class BodyPartsRotate : MonoBehaviour {
        [SerializeField] private float speed;
        public Transform target;
        private Vector2 direction;


        private void FixedUpdate() {
            if (target) enabled = false;
            
            // direction = target.position - transform.position;
            // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            // transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed * Time.deltaTime);

            transform.DOLookAt(target.position, Time.fixedDeltaTime);
        }
    }
}