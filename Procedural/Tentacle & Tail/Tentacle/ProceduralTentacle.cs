﻿using UnityEngine;

namespace Toolbox.Procedural.Tentacle {
    /// <summary>
    ///     tentacule fait a partir d'un linerenderer a placer sur le point ou elle doit etre connecté
    ///     \
    ///     #---#   #---#
    ///     | 1 |---| 2 |
    ///     #---#   #---#
    ///     /
    ///     1   tete        =>  ProceduralTentacleWiggle et LineRenderer
    ///     2   direction   =>  definie la direction avec sa rotation
    /// </summary>
    public class ProceduralTentacle : MonoBehaviour {
        [SerializeField] private int length;

        [SerializeField] private Transform targetDir;
        [SerializeField] private float targetDist;
        [SerializeField] private float smoothSpeed;
        [SerializeField] private float trailSpeed;

        [Space] [SerializeField] private LineRenderer lineRenderer;


        private Vector3[] _segmentPoses;
        private Vector3[] _segmentV;

        private void Start() {
            lineRenderer.positionCount = length;

            _segmentPoses = new Vector3[length];
            _segmentV = new Vector3[length];
        }

        private void Update() {
            _segmentPoses[0] = targetDir.position;

            for (int i = 1; i < _segmentPoses.Length; i++)
                _segmentPoses[i] = Vector3.SmoothDamp(_segmentPoses[i],
                    _segmentPoses[i - 1] + targetDir.right * targetDist,
                    ref _segmentV[i], smoothSpeed + i / trailSpeed);

            lineRenderer.SetPositions(_segmentPoses);
        }
    }
}