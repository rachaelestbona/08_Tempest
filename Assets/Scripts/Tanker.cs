using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tanker : MonoBehaviour, IShipBase {

	public GameObject flipperPrefab;
	public GameObject explodePrefab;
	public Rigidbody projectilePrefab;
	public Transform fireTransform;

	public float fireDelay = 1.5f;
	public float fireSpeed = 10f;
	public AudioClip soundDeath;
	public AudioClip soundFire;
	[HideInInspector] public float moveSpeed = 1f;
	[HideInInspector] public MapLine curMapLine;

	private MapManager _mapManager;
	private GameManager _gameManager;
	private float _lastFire;
	private Rigidbody _rigidbody;
	private AudioSource _audioSource;
	private GameObject _playerRef;

	// Use this for initialization
	void Start () {
		_rigidbody = GetComponent<Rigidbody> ();
		_mapManager = GameObject.Find ("MapManager").GetComponent<MapManager> ();
		_gameManager = GameObject.Find ("GameManager").GetComponent<GameManager> ();
		_audioSource = GetComponent<AudioSource> ();
		StartCoroutine (FireCoroutine ());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate () {
		Move ();

		if (transform.position.z < 3)
			OnDeath ();
	}

	void Move(){
		Vector3 newPos = curMapLine.GetMidPoint();
		newPos = newPos + new Vector3 (0f, 0f, transform.position.z - moveSpeed * Time.deltaTime);

		_rigidbody.MovePosition (newPos);

		Vector3 curDirVec = curMapLine.GetDirectionVector ();
		Vector3 newDirVec = new Vector3 (-curDirVec.y, curDirVec.x, 0);
		//print (Quaternion.Euler(newDirVec));
		_rigidbody.MoveRotation (Quaternion.LookRotation(new Vector3(0f,0f,1f), newDirVec));
	}

	public IEnumerator FireCoroutine() {
		yield return new WaitForSeconds (fireDelay);
		Fire ();
		StartCoroutine (FireCoroutine ());
	}

	public void Fire(){
		_audioSource.clip = soundFire;
		_audioSource.Play ();
		Rigidbody shellInstance = Instantiate (projectilePrefab, fireTransform.position, fireTransform.rotation) as Rigidbody;
		//shellInstance.GetComponent<PlayerBullet> ().SetShip (gameObject);
		shellInstance.velocity = -fireSpeed * (fireTransform.forward); 
		shellInstance.GetComponent<TankerBullet> ().SetShip (gameObject);
	}

	public void TakeDamage(int dmg) { 
		OnDeath ();
	}

	public void OnDeath() {

		// TODO spawn two flippers
		_gameManager.TankerDestroyed();

		if (curMapLine.leftLine != null) {
			SpawnFlipper (curMapLine.leftLine);
		} else {
			SpawnFlipper (curMapLine);
		}
		if (curMapLine.rightLine != null) {
			SpawnFlipper (curMapLine.rightLine);
		} else {
			SpawnFlipper (curMapLine);
		}

		GameObject newExplosion = Instantiate (explodePrefab, gameObject.transform.position, gameObject.transform.rotation);
		AudioSource explosionSource = newExplosion.GetComponent<AudioSource> ();
		explosionSource.clip = soundDeath;
		explosionSource.Play ();

		Destroy (gameObject);
	}

	void SpawnFlipper(MapLine newMapLine) {
		Vector3 curDirVec = newMapLine.GetDirectionVector ();
		Vector3 newDirVec = new Vector3 (-curDirVec.y, curDirVec.x, 0);
		GameObject newShip = Instantiate (flipperPrefab, newMapLine.GetMidPoint() + new Vector3 (0, 0, transform.position.z + 1f), Quaternion.LookRotation(new Vector3(0f,0f,1f), newDirVec));
		newShip.GetComponent<Flipper>().SetMapLine (newMapLine);
		newShip.GetComponent<Flipper>().movementForce = _gameManager.currentRound * _gameManager.speedMulti;
	}
}
