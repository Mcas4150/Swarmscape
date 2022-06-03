using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFlowfield : MonoBehaviour
{

    FastNoise _fastNoise;
    public Vector3Int _gridSize;
    public float _cellSize;
    public Vector3[,,] _flowfieldDirection;
    public float _increment;
    public Vector3 _offset, _offsetSpeed;



    public GameObject _particlePrefab;
    public int _amountOfParticles;
    [HideInInspector]
    public List<flowfieldParticle> _particles;
    public float _particleScale;





    // Start is called before the first frame update
    void Start()
    {
        _flowfieldDirection = new Vector3[_gridSize.x, _gridSize.y, _gridSize.z];
        _fastNoise = new FastNoise();
        _particles = new List<flowfieldParticle>();
        for (int i = 0; i < -_amountOfParticles; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(this.transform.position.x, this.transform.position.x + _gridSize.x * _cellSize),
                Random.Range(this.transform.position.y, this.transform.position.y + _gridSize.y * _cellSize),
                Random.Range(this.transform.position.z, this.transform.position.z + _gridSize.z * _cellSize)
                );

            GameObject particleInstance = (GameObject)Instantiate(_particlePrefab);
            particleInstance.transform.position = randomPos;
            particleInstance.transform.parent = this.transform;
            particleInstance.transform.localScale = new Vector3(_particleScale, _particleScale, _particleScale);
            _particles.Add(particleInstance.GetComponent<flowfieldParticle>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateFlowfieldDirections();
    }


    void CalculateFlowfieldDirections()
    {


        float xOff = 0f;

        for (int x = 0; x < _gridSize.x; x++)
        {
            float yOff = 0f;
            for (int y = 0; y < _gridSize.y; y++)
            {
                float zOff = 0f;
                for (int z = 0; z < _gridSize.z; z++)
                {
                    float noise = _fastNoise.GetSimplex(xOff + _offset.x, yOff + _offset.y, zOff + _offset.z) + 1;
                    Vector3 noiseDirection = new Vector3(Mathf.Cos(noise * Mathf.PI), Mathf.Sin(noise * Mathf.PI), Mathf.Cos(noise * Mathf.PI));
                    _flowfieldDirection[x, y, z] = Vector3.Normalize(noiseDirection);

                    zOff += _increment;


                }

                yOff += _increment;
            }
            xOff += _increment;
        }


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(
            transform.position + new Vector3((_gridSize.x * _cellSize) * 0.5f, (_gridSize.y * _cellSize) * 0.5f, (_gridSize.z * _cellSize) * 0.5f),
            new Vector3(_gridSize.x * _cellSize, _gridSize.y * _cellSize, _gridSize.z * _cellSize));
    }
}
