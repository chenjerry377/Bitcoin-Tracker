using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;

class Program {

  public static List<String> getData() {

    TcpClient client = new TcpClient("api.coindesk.com",80);
    NetworkStream stream = client.GetStream();
    StreamWriter writer = new StreamWriter(stream);
    StreamReader reader = new StreamReader(stream);

    writer.WriteLine("GET http://api.coindesk.com/v1/bpi/currentprice.json HTTP/1.0\n\n");   
    writer.Flush();
  
    List<String> list = new List<String>();
  
    while(!reader.EndOfStream) {
      string line = reader.ReadLine();
      list.Add(line);
    }

    stream.Close();
    return(list);
      
  }
  
  public static float getDollarPrice(List<string> lines) {
    bool header=true;
    String json="";
    foreach(string line in lines) {
      if(line.Equals("")) {
        header=false;
        continue;
      }
      if(header==false) {
        json=line;
        break;
      }
    }
    //Console.WriteLine("Json: "+json);
    String[] jsonParts=json.Split(":");
    String priceLine=jsonParts[19];
    String justPrice=priceLine.Replace("},\"GBP\"","");
    float price=Convert.ToSingle(justPrice);
    return price;
  }
  
  public static void buyBitCoin(float price) {
  
    StreamReader reader = new StreamReader("initialInvestmentUSD.txt");
    StreamWriter writer = new StreamWriter("clientBC.txt");

    while(!reader.EndOfStream) {
      string[] arr = reader.ReadLine().Split(":");
      float bit = Convert.ToSingle(arr[1]) / price;
      writer.WriteLine($"{arr[0]}:{bit}");
    }
    reader.Close();
    writer.Close();

    //string readText = File.ReadAllText("clientBC.txt"); 
    //Console.WriteLine(readText);
      
  }
  
  public static float getPersonFromFile(string person, string file) {
  
    StreamReader reader = new StreamReader(file);
  
    while(!reader.EndOfStream) {
      string[] arr = reader.ReadLine().Split(":");
      if(arr[0].Equals(person)) {
        reader.Close();
        return(Convert.ToSingle(arr[1]));
      }
    }

    throw new PersonNotFound("Person Not Found");
    
  }
  
  public static void getCurrentValue(float bitcoin) {
      
    StreamReader reader = new StreamReader("clientBC.txt");
      
    while(!reader.EndOfStream) {
      string[] arr = reader.ReadLine().Split(":");
      float usd = Convert.ToSingle(arr[1]) * bitcoin;
      Console.WriteLine($"{arr[0]}:{usd}");
    }

    reader.Close();
      
  }

  public static void Main (string[] args) {

    try {
      while(true) {
        float currentBitPrice = getDollarPrice(getData());
        Console.WriteLine($"One BitCoin is currently worth {currentBitPrice}");
  
        Console.WriteLine("1. Buy Bitcoin\n2. See everyones current value in USD\n3. See one persons gain/loss\n4. Quit");
        int answer = Convert.ToInt32(Console.ReadLine());
  
        if(answer == 1) {
          buyBitCoin(currentBitPrice);
        } else if(answer == 2) {
          getCurrentValue(currentBitPrice);
        } else if(answer == 3) {
          Console.WriteLine("Enter a name");
          string name = Console.ReadLine();
  
          float original = getPersonFromFile(name, "initialInvestmentUSD.txt");
          float bits = getPersonFromFile(name, "clientBC.txt");
          float present = bits * currentBitPrice;
          float change = present - original;
          
          Console.WriteLine($"{name}:\nOriginal Investment: ${original}\nNumber of bitcoins: {bits}\nCurrent Value: ${present}\nChange in value: ${change}");
        } else if(answer == 4) {
          break;
        }
      }
    } catch (PersonNotFound e) {
      Console.WriteLine(e.Message);
    } catch (Exception e) {
      Console.WriteLine(e.Message);
    }
  }
}

class PersonNotFound : Exception {

  public PersonNotFound() {}

  public PersonNotFound(string message) : base(message) {}
  
}