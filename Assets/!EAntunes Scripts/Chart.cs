using System;
using UnityEngine;
using UnityEngine.VFX;

public class Chart : MonoBehaviour
{
    public TextAsset csvFile;

    public bool scrollChart = false;
    public bool useWorldScaling = false;
    [Min(0)] public int numBarRange = 9;
    int oldNumBarRange = 9;
    [Min(1)] public int middleBar = 5;
    int oldMiddleBar = 5;

    BarObjectContainer barObjectContainer = new BarObjectContainer();

    Vector3 parentGOMag = Vector3.zero;
    float spacing = 0f;

    [SerializeField] Material barMaterial;

    VisualEffect vfx;
    [SerializeField] VisualEffectAsset vfxAsset;

    public class BarObjectContainer
    {
        public Transform parentGOTransform;
        public BarObject[] barObjects;
        public float currentHigh = 0f;
        public float currentLow = Mathf.Infinity;
        public float dailyOpen = 0f;
        public float dailyHigh = 0f;
        public float dailyLow = Mathf.Infinity;
        public float dailyClose = 0f;
    }

    public enum DecimalPlaceNames
    {
        Hundreths,
        Tenths,
        Ones,
        Tens,
        Hundreds,
        Thousands,
        TenThousands,
        HundredThousands,
        Millions
    }

    private void Start()
    {
        if(csvFile != null)
        {
            BuildObjectContainer();
            UpdateBarCount();
            Rescale();
        } 
        else
        {
            Destroy(this);
        }

    }

    private void Update()
    {
        if (scrollChart && Time.frameCount % 90 == 0)
        {
            ++middleBar;
            var barID = middleBar + numBarRange;
            var maxIndex = barObjectContainer.barObjects.Length;
            var bar = barObjectContainer.barObjects[Math.Min(barID, maxIndex - 1)];

            char[] numberChars = bar.barData.close.ToString().ToCharArray();
            int[] nums = new int[numberChars.Length - 1];

            int counter = 0;
            for (int i = numberChars.Length - 1; i >= 0; i--)
            {
                if (numberChars[i] != '.')
                {
                    var newInt = char.GetNumericValue(numberChars[i]);

                    nums[counter++] = (int)newInt;
                }

            }

            var intLength = nums.Length;
            vfx.SetInt("IntLength", intLength);

            foreach (DecimalPlaceNames name in System.Enum.GetValues(typeof(DecimalPlaceNames)))
            {
                if ((int)name < intLength)
                {
                    UpdateDecimalPlace(name, nums[(int)name]);
                }

                if ((int)name >= intLength)
                {
                    UpdateDecimalPlace(name);
                }
            }

            vfx.SetFloat("LastPrice", bar.barData.close);
            print("Lastprice is " + bar.barData.close);

            vfx.SetFloat("ChartHigh", barObjectContainer.currentHigh);
            vfx.SetFloat("ChartLow", barObjectContainer.currentLow);
        }

        if (middleBar != oldMiddleBar)
        {
            UpdateBarCount();
            Rescale();
            oldMiddleBar = middleBar;

        }
        if (numBarRange != oldNumBarRange)
        {
            UpdateBarCount();
            Rescale();
            oldNumBarRange = numBarRange;
        }
    }

    void UpdateDecimalPlace(DecimalPlaceNames decimalPlace)
    {
        UpdateDecimalPlace(decimalPlace, -1);
    }

    void UpdateDecimalPlace(DecimalPlaceNames decimalPlace, int newInt)
    {
        print("NewInt " + newInt);
        switch (decimalPlace)
        {
            case DecimalPlaceNames.Hundreths:
                vfx.SetInt("HundrethsInt", newInt);
                break;
            case DecimalPlaceNames.Tenths:
                vfx.SetInt("TenthsInt", newInt);

                break;
            case DecimalPlaceNames.Ones:
                vfx.SetInt("OnesInt", newInt);

                break;
            case DecimalPlaceNames.Tens:
                vfx.SetInt("TensInt", newInt);

                break;
            case DecimalPlaceNames.Hundreds:
                vfx.SetInt("HundredsInt", newInt);

                break;
            case DecimalPlaceNames.Thousands:
                vfx.SetInt("ThousandsInt", newInt);

                break;
            case DecimalPlaceNames.TenThousands:
                vfx.SetInt("TenThousandsInt", newInt);

                break;
            case DecimalPlaceNames.HundredThousands:
                vfx.SetInt("HundredThousandsInt", newInt);

                break;
            case DecimalPlaceNames.Millions:
                vfx.SetInt("MillionsInt", newInt);

                break;
        }
    }

    private void UpdateBarCount()
    {
        //The +1's in the for loop is to ensure odd number of bars on screen.

        var barObjects = barObjectContainer.barObjects;
        var barObjsCount = barObjects.Length;
        barObjectContainer.currentHigh = 0;
        barObjectContainer.currentLow = Mathf.Infinity;


        for (int i = Mathf.Max(0, oldMiddleBar - oldNumBarRange); i < Mathf.Min(barObjsCount, oldMiddleBar + oldNumBarRange + 1); i++)
        {
            if (i >= 0 || i < barObjsCount)
            {
                barObjects[i].barObjectPrimitive.SetActive(false);
            }
        }

        for (int i = Mathf.Max(0, middleBar - numBarRange); i < Mathf.Min(barObjsCount, middleBar + numBarRange + 1); i++)
        {
            if (i >= 0 || i < barObjsCount)
            {
                if (barObjects[i].barData.high > barObjectContainer.currentHigh)
                {
                    barObjectContainer.currentHigh = barObjects[i].barData.high;
                }
                if (barObjects[i].barData.low < barObjectContainer.currentLow)
                {
                    barObjectContainer.currentLow = barObjects[i].barData.low;
                }


                barObjects[i].barObjectPrimitive.SetActive(true);
            }
        }
    }

    void Rescale()
    {
        foreach (var barObject in barObjectContainer.barObjects)
        {
            Rescale(barObject);
        }
    }

    void Rescale(BarObject barObject)
    {
        float priceToHighLowRatio = 0f;
        float yPos = 0f;

        if (useWorldScaling)
        {
            //Entire date range scaling
            priceToHighLowRatio = (barObject.barData.high - barObject.barData.low) / (barObjectContainer.dailyHigh - barObjectContainer.dailyLow);
        }
        else
        {
            priceToHighLowRatio = (barObject.barData.high - barObject.barData.low) / (barObjectContainer.currentHigh - barObjectContainer.currentLow);
        }

        var scaledYValue = priceToHighLowRatio;

        spacing = 1f / ((numBarRange * 2) + 1);

        var barMidPoint = ((barObject.barData.high - barObject.barData.low) / 2);
        var barMidPrice = barMidPoint + barObject.barData.low;
        var midPriceRange = 0f;
        var priceRangeOnScreen = 0f;

        if (useWorldScaling)
        {
            midPriceRange = barMidPrice - barObjectContainer.dailyLow;
            priceRangeOnScreen = barObjectContainer.dailyHigh - barObjectContainer.dailyLow;
         
            yPos = ((midPriceRange / priceRangeOnScreen) - .5f);
        }
        else
        {
            midPriceRange = barMidPrice - barObjectContainer.currentLow;
            priceRangeOnScreen = barObjectContainer.currentHigh - barObjectContainer.currentLow;

            if (barObject.barID >= Mathf.Max(0, middleBar - numBarRange) && barObject.barID < (middleBar + numBarRange + 1) && barObject.barData.high > barObjectContainer.currentHigh)
            {
                print("High " + barObject.barData.high + " > " + barObjectContainer.currentHigh);
            }

            yPos = ((midPriceRange / priceRangeOnScreen) - .5f);
        }

        barObject.barObjectPrimitive.transform.localScale = new Vector3(spacing * .25f, scaledYValue * .5f, spacing * .25f); //multiple here is to make sure the bar width is smaller than the spacing apart

        var xOffset = 0;

        if (barObject.barID == middleBar)
        {
            xOffset = 0;
        }
        if (barObject.barID > middleBar)
        {
            xOffset = (barObject.barID - middleBar);
        }
        else
        {
            xOffset = -(middleBar - barObject.barID);
        }

        barObject.barObjectPrimitive.transform.localPosition = new Vector3(spacing * -xOffset, yPos, 0f);

        //For if you want the bars to move around outside the container cube like a train
        //barObject.barObjectPrimitive.transform.localPosition = new Vector3(spacing * barObject.barID, yPos, 0f); 
    }


    void BuildObjectContainer()
    {
        var intervalBars = ReadExcel.ReadCSV(csvFile);
        barObjectContainer = new BarObjectContainer();
        barObjectContainer.barObjects = new BarObject[intervalBars.Length];

        var parentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);

        parentGO.transform.position = Vector3.zero;
        parentGO.transform.localScale = new Vector3(1f, 1f, .1f);

        var parentGORend = parentGO.GetComponent<Renderer>();

        if (parentGORend != null)
        {
            parentGOMag = parentGORend.bounds.size;
        }

        barObjectContainer.parentGOTransform = parentGO.transform;

        vfx = parentGO.AddComponent<VisualEffect>();

        if (vfxAsset == null)
        {
            print("No VFX Asset on " + this.name);
            return;
        }
    
        vfx.visualEffectAsset = vfxAsset;
        var arrayLength = intervalBars.Length;

        for(int i = 0; i < arrayLength; i++)
        {
            var barData = intervalBars[i];
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bar.transform.parent = parentGO.transform;

            var rend = bar.GetComponent<Renderer>();

            rend.material = barMaterial;
            var mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);
            rend.SetPropertyBlock(mpb);
            mpb.SetFloat("_Open", barData.open);
            mpb.SetFloat("_High", barData.high);
            mpb.SetFloat("_Low", barData.low);
            mpb.SetFloat("_Close", barData.close);
            rend.SetPropertyBlock(mpb);

            var barObject = new BarObject(i, bar, barData, mpb);

            barObjectContainer.barObjects[i] = barObject;

            if (i > Mathf.Min(arrayLength, middleBar + numBarRange + 1) || i < Mathf.Max(0, middleBar - numBarRange))
            {
                bar.SetActive(false);
            }

            UpdateChartWideOHL(barData);

            if (i == arrayLength)
            {
                barObjectContainer.dailyClose = barData.close;
            }

        }
    }

    void UpdateChartWideOHL(BarData barData)
    {
        if (barObjectContainer.dailyOpen == 0)
        {
            barObjectContainer.dailyOpen = barData.open;
        }

        if (barObjectContainer.dailyHigh < barData.high)
        {
            barObjectContainer.dailyHigh = barData.high;
        }

        if (barObjectContainer.dailyLow > barData.low)
        {
            barObjectContainer.dailyLow = barData.low;
        }
    }
}
