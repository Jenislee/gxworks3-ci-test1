using System;
using ActUtlTypeLib;
namespace PLCscript_test
{
    internal class Program
    {
        public static int WriteFloatToPlc(ref ActUtlType plc,string startAddress, float value)
        {
            try
            {
                byte[] floatBytes = BitConverter.GetBytes(value);
                short lowerWord = BitConverter.ToInt16(floatBytes, 0);
                short upperWord = BitConverter.ToInt16(floatBytes, 2);

                // Write to consecutive D registers
                plc.SetDevice(startAddress, lowerWord);
                plc.SetDevice($"D{int.Parse(startAddress.Substring(1)) + 1}", upperWord);

                Console.WriteLine($"✅ Write Success: {value} → {startAddress} ({lowerWord}, {upperWord})");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return -1;
            }
        }
        public static int ReadFloatToPlc(ref ActUtlType plc, string startAddress, out float resultValue)
        {
            resultValue = 0.0f;
            try
            {

                // Read lower and upper words
                int lowerWord, upperWord;
                plc.GetDevice(startAddress, out lowerWord);
                plc.GetDevice($"D{int.Parse(startAddress.Substring(1)) + 1}", out upperWord);

                // Convert back to float
                byte[] floatBytes = new byte[4];
                BitConverter.GetBytes((short)lowerWord).CopyTo(floatBytes, 0);
                BitConverter.GetBytes((short)upperWord).CopyTo(floatBytes, 2);
                resultValue = BitConverter.ToSingle(floatBytes, 0);

                Console.WriteLine($"✅ Read Success: {startAddress} → {resultValue} ({lowerWord}, {upperWord})");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return -1;
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            ActUtlType plc = new ActUtlType();
            plc.ActLogicalStationNumber = 2; // Set your station number
            try
            {
                int result = plc.Open();
                int Y_out=0;
                short D304_out;
                int[] D304_int = new int[2];
                if(result != 0)
                {
                    Console.WriteLine("Connection failed!");
                }
                else
                {
                    Console.WriteLine("✅ Connected to PLC successfully!");
                    plc.GetDevice("Y103", out Y_out);
                    Console.WriteLine("Y_out when M2=0:{0}", Y_out);
                    plc.SetDevice("M2", 1);
                    System.Threading.Thread.Sleep(100);
                    plc.GetDevice("Y103", out Y_out);
                    Console.WriteLine("Y_out when M2=1:{0}", Y_out);
                    plc.SetDevice("M2", 0);

                    //SUM function test for float(float data)

                    //Write float data to PLC
                    float value1 = 18.5f;
                    byte[] floatBytes = BitConverter.GetBytes(value1);
                    int lowerWord = BitConverter.ToInt16(floatBytes, 0); // LSB
                    int upperWord = BitConverter.ToInt16(floatBytes, 2); // MSB

                    
                    plc.SetDevice("D301", upperWord);
                    plc.SetDevice("D300", lowerWord);// Write to D200 (lower) and D201 (upper)
                    Console.WriteLine($"Written to PLC: D300 = {lowerWord}, D301 = {upperWord}");


                    float value2 = 22.8f;
                    floatBytes = BitConverter.GetBytes(value2);
                    lowerWord = BitConverter.ToInt16(floatBytes, 0); // LSB
                    upperWord = BitConverter.ToInt16(floatBytes, 2); // MSB

                    plc.SetDevice("D311", upperWord);
                    plc.SetDevice("D310", lowerWord);// Write to D200 (lower) and D201 (upper)
                    
                    Console.WriteLine($"Written to PLC: D310 = {lowerWord}, D311 = {upperWord}");
                    System.Threading.Thread.Sleep(100);
                    //Read float data from PLC
                    int lowerWordint, upperWordint;
                    plc.GetDevice("D321", out upperWordint);
                    plc.GetDevice("D320", out lowerWordint);
                    

                    Console.WriteLine($"Read from PLC: D320 = {lowerWordint}, D321 = {upperWordint}");

                    // Convert back to float
                    byte[] floatBytesget = new byte[4];
                    BitConverter.GetBytes((short)lowerWordint).CopyTo(floatBytesget, 0);
                    BitConverter.GetBytes((short)upperWordint).CopyTo(floatBytesget, 2);
                    float reconstructedFloat = BitConverter.ToSingle(floatBytesget, 0);

                    Console.WriteLine($"Reconstructed Float Value: {reconstructedFloat}");

                    float value3 = 12.8f, value4 = 24.9f;
                    WriteFloatToPlc(ref plc, "D300", value3);
                    WriteFloatToPlc(ref plc, "D310", value4);
                    ReadFloatToPlc(ref plc, "D320", out float resultFloat);
                    Console.WriteLine($" Float Value: {resultFloat}");
                    if (resultFloat == value3+value4)
                        Console.WriteLine("Test Passed!");
                    else
                        Console.WriteLine($"Test Failed! Expected {value3 + value4} but got {resultFloat}");



                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                plc.Close();
            }

        }
        private static float IntArrayToFloat(int[] data)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(data[0] & 0xFF);
            bytes[1] = (byte)(data[0] >> 8 & 0xFF);
            bytes[2] = (byte)(data[1] & 0xFF);
            bytes[3] = (byte)(data[1] >> 8 & 0xFF);
            float Result = BitConverter.ToSingle(bytes, 0);
            return Result;
        }
    }
}
