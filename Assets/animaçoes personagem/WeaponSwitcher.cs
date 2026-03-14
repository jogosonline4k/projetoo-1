using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapons")]
    public GameObject pistol;
    public GameObject shotgun;

    [Header("UI")]
    public Image pistolIndicator;   // Indicador da pistola
    public Image shotgunIndicator;  // Indicador da escopeta

    private GameObject activeWeapon;

    void Start()
    {
        activeWeapon = pistol;

        pistol.SetActive(true);
        shotgun.SetActive(false);

        UpdateIndicators();
        UpdateAmmoUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(pistol);
            UpdateIndicators();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(shotgun);
            UpdateIndicators();
        }
    }

    void SwitchWeapon(GameObject newWeapon)
    {
        if (activeWeapon == newWeapon)
            return;

        activeWeapon.SetActive(false);
        newWeapon.SetActive(true);
        activeWeapon = newWeapon;

        UpdateAmmoUI();
    }

    void UpdateIndicators()
    {
        // Ativa apenas o indicador correspondente
        pistolIndicator.gameObject.SetActive(activeWeapon == pistol);
        shotgunIndicator.gameObject.SetActive(activeWeapon == shotgun);
    }

    void UpdateAmmoUI()
    {
        // Pistola
        Gun gun = activeWeapon.GetComponent<Gun>();
        if (gun != null)
        {
            gun.UpdateAmmoUI();
            return;
        }

        // Espingarda
        Escopeta escopeta = activeWeapon.GetComponent<Escopeta>();
        if (escopeta != null)
        {
            escopeta.UpdateAmmoUI();
            return;
        }
    }
}
