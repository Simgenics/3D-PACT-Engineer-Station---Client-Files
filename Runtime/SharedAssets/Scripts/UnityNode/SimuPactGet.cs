using DisplayServer;
using Unity.VisualScripting;


public class SimuPactGet : Unit
{

    [DoNotSerialize] // No need to serialize ports.
    public ControlInput input; //Adding the ControlInput port variable

    [DoNotSerialize] // No need to serialize ports.
    public ControlOutput output;//Adding the ControlOutput port variable.

    [DoNotSerialize] // No need to serialize ports
    public ValueInput ValuID; // Adding the ValueInput variable for myValueB

    [DoNotSerialize] // No need to serialize ports
    public ValueOutput result; // Adding the ValueOutput variable for result


    private double resultValue = double.NaN;
    private Communications Coms;
    private PointData pdDisplayPosition;

    protected override void Definition() //The method to set what our node will be doing.
    {
        Coms = Communications.Instance;

        input = ControlInput("input", (flow) =>
        {
            if (Coms != null)
            {
                if (Coms.IsConnected)
                {
                    // if (pdDisplayPosition == null)
                        pdDisplayPosition = Coms?.GetDataPoint(flow.GetValue<string>(ValuID));
                    if (pdDisplayPosition != null)
                        resultValue = pdDisplayPosition.Value;
                }
                else
                {
                    resultValue = double.NaN;
                }
            }
            else
            {
                Coms = Communications.Instance;

                if (Coms != null)
                {
                    if (Coms.IsConnected)
                    {
                        // if (pdDisplayPosition == null)
                            pdDisplayPosition = Coms?.GetDataPoint(flow.GetValue<string>(ValuID));
                        if (pdDisplayPosition != null)
                            resultValue = pdDisplayPosition.Value;
                    }
                }
            }
            return output;
        });
        output = ControlOutput("output");


        //Making the myValueB input value port visible, setting the port label name to myValueB and setting its default value to an empty string.
        ValuID = ValueInput<string>("ID", string.Empty);
        //Making the result output value port visible, setting the port label name to result and setting its default value to the resultValue variable.
        result = ValueOutput<double>("result", (flow) => { return resultValue; });
    }
}
