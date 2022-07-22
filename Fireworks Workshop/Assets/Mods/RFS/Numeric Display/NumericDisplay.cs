using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class NumericDisplay : MonoBehaviour
{

    public float Number = 0;
    public TMP_Text DisplayText;
    private string fmtstring = "00";
    private string fmtstring2 = "0";
    private string fmtstring3 = "000";
    private string formatstring4 = "0000";
    //public bool SingleDigit = false;
    //public bool TrippleDigit = false;
    public digits NumberOfDigits = digits.doubleDigit;
    [HideInInspector]
    public bool up = false;
    [HideInInspector]
    public bool down = false;
    public UnityEvent<float> updateid;
    

    public enum digits
    {
        singleDigit,
        doubleDigit,
        tripleDigit,
        quadDigit,
        infDigit,
    }


    private void OnValidate()
    {
        if ((UnityEngine.Object)this.DisplayText == (UnityEngine.Object)null)
        {
            Debug.LogError((object)"Missing Display TMP_Text Asset in Numeric Display!");
            DisplayText = this.GetComponentInChildren<TMP_Text>();
        }
        UpdateDisplay(Number);
    }

    public void Cancel()
    {
        up = false;
        down = false;
        StopAllCoroutines();
    }

    public void UpButton()
    {
        if (NumberOfDigits == digits.singleDigit)
        {
            if (Number < 9)
            {
                up = true;
                Number += 1;
                UpdateDisplay(Number);
                StartCoroutine(Count(true));
            }
        }
        else if (NumberOfDigits == digits.doubleDigit)
        {
            if (Number < 99)
            {
                up = true;
                Number += 1;
                UpdateDisplay(Number);
                StartCoroutine(Count(true));
            }
        }
        else if (NumberOfDigits == digits.tripleDigit)
        {
            if (Number < 999)
            {
                up = true;
                Number += 1;
                UpdateDisplay(Number);
                StartCoroutine(Count(true));
            }
        }
        else if (NumberOfDigits == digits.quadDigit)
        {
            if (Number < 9999)
            {
                up = true;
                Number += 1;
                UpdateDisplay(Number);
                StartCoroutine(Count(true));
            }
        }
    }
    public void DownButton()
    {
        if (NumberOfDigits != digits.infDigit)
        {
            if (Number > 0)
            {
                down = true;
                Number -= 1;
                UpdateDisplay(Number);
                StartCoroutine(Count(false));
            }
        }
    }

    IEnumerator Count(bool direction)
    {
        if (direction)
        {
            yield return new WaitForSeconds(1f);
            if (up == false)
            {
                yield break;
            }
            do
            {
                if (NumberOfDigits == digits.singleDigit)
                {
                    if (Number < 9)
                    {
                        Number += 1;
                    }
                }
                else if (NumberOfDigits == digits.doubleDigit)
                {
                    if (Number < 99)
                    {
                        Number += 1;
                    }
                }
                else if (NumberOfDigits == digits.tripleDigit)
                {
                    if (Number < 999)
                    {
                        Number += 1;
                    }
                }
                else if (NumberOfDigits == digits.quadDigit)
                {
                    if (Number < 9999)
                    {
                        Number += 1;
                    }
                }

                UpdateDisplay(Number);
                yield return new WaitForSeconds(0.1f);
            } while (up == true);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            if (down == false)
            {
                yield break;
            }
            do
            {
                if (Number > 0)
                {
                    Number -= 1;
                }
                UpdateDisplay(Number);
                yield return new WaitForSeconds(0.15f);
            } while (down == true);
        }
    }

    public void UpdateDisplay(float chnl)
    {
        if (DisplayText != null)
        {
            if (NumberOfDigits == digits.singleDigit)
            {
                if (chnl >= 0 && chnl <= 9)
                {
                    Number = chnl;
                    string displayint = chnl.ToString(fmtstring2);
                    DisplayText.text = displayint;
                }
                else if (chnl > 9)
                {
                    DisplayText.text = "9";
                }
                else
                {
                    DisplayText.text = "0";
                }
            }
            else if (NumberOfDigits == digits.doubleDigit)
            {
                if (chnl >= 0 && chnl <= 99)
                {
                    Number = chnl;
                    string displayint = chnl.ToString(fmtstring);
                    DisplayText.text = displayint;
                }
                else if (chnl > 99)
                {
                    DisplayText.text = "99";
                }
                else
                {
                    DisplayText.text = "00";
                }
            }
            else if (NumberOfDigits == digits.tripleDigit)
            {
                if (chnl >= 0 && chnl <= 999)
                {
                    Number = chnl;
                    string displayint = chnl.ToString(fmtstring3);
                    DisplayText.text = displayint;
                }
                else if (chnl > 999)
                {
                    DisplayText.text = "999";
                }
                else
                {
                    DisplayText.text = "000";
                }
            }
            else if (NumberOfDigits == digits.quadDigit)
            {
                //Debug.Log("channel: " + chnl + " string: " + chnl.ToString(formatstring4));
                if (chnl >= 0 && chnl <= 9999)
                {
                    Number = chnl;
                    string displayint = chnl.ToString(formatstring4);
                    DisplayText.text = displayint;
                }
                else if (chnl > 9999)
                {
                    DisplayText.text = "9999";
                }
                else
                {
                    DisplayText.text = "0000";
                }
            }
            else if (NumberOfDigits == digits.infDigit)
            {
                //Debug.Log("channel: " + chnl + " string: " + chnl.ToString(formatstring4));
                    Number = chnl;
                    string displayint = chnl.ToString();
                    DisplayText.text = displayint;
            }
        }
        updateid.Invoke(Number);
    }
}
