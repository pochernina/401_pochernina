import './App.css';
import React from "react";
import axios from 'axios';

const BACKEND_BASE_ADDRESS = 'https://localhost:5001/api/images'
const EMOTIONS = ["neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt"]

class App extends React.Component {

  constructor(props) {
    super(props);
    this.state = {
      selectedFile: null,
      selectedEmotion: "",
      images_with_results: [],
    };
    this.get_images_and_results = this.get_images_and_results.bind(this);
    this.handleSubmitFiles = this.handleSubmitFiles.bind(this);
    this.handleFileSelect = this.handleFileSelect.bind(this);
    this.convertToBase64 = this.convertToBase64.bind(this);
    this.streamToString = this.streamToString.bind(this);
    this.handleEmotionSelect = this.handleEmotionSelect.bind(this);
  }

  componentDidMount(props) {
    this.get_images_and_results()
  }

  convertToBase64(file) {
    return new Promise((resolve, reject) => {
        const fileReader = new FileReader();
        fileReader.readAsDataURL(file);

        fileReader.onload = () => {
            resolve(fileReader.result);
        };

        fileReader.onerror = (error) => {
            reject(error);
        };
    });
  };


  streamToString (stream) {
    const chunks = [];
    return new Promise((resolve, reject) => {
      stream.on('data', (chunk) => chunks.push(Buffer.from(chunk)));
      stream.on('error', (err) => reject(err));
      stream.on('end', () => resolve(Buffer.concat(chunks).toString('utf8')));
    })
  }
  

  async handleSubmitFiles(event) {
    event.preventDefault()

    let img_base64 = await this.convertToBase64(this.state.selectedFile);
    img_base64 = img_base64.replace(/data:image\/.*;base64,/, "")
    try {
      const response = await axios({
        method: "POST",
        url: BACKEND_BASE_ADDRESS,
        data: img_base64,
        headers: { 
          'Access-Control-Allow-Origin': '*',
          'Content-Type': 'application/json',
        },
        credentials: 'same-origin',
      });
      this.get_images_and_results()
    } catch(error) {
      console.log(error)
    }
  }

  handleEmotionSelect(event) {
    this.setState({selectedEmotion: event.target.value}, () => {
      this.get_images_and_results()
    })
  }

  handleFileSelect(event){
    this.setState({
      selectedFile: event.target.files[0]
    })
  }

  async get_images_and_results() {

    let images_with_results = []

    if (this.state.selectedEmotion === '') {
      // show all
      let images = await fetch(BACKEND_BASE_ADDRESS + '/images_db',  {mode: 'cors'})
      let results = await fetch(BACKEND_BASE_ADDRESS + '/results_db',  {mode: 'cors'})
      images = await images.json()
      results = await results.json()
      for (let i in images) {
        images_with_results.push({
          image: images[i],
          result: results[i].split(';'),
        })
      }
    } else {
      // get sorted by emotion
      let response = await fetch(BACKEND_BASE_ADDRESS + '?emotion=' + this.state.selectedEmotion ,  {mode: 'cors'})
      response = await response.json()
      for (let img in response) {
        images_with_results.push({
          image: img,
          result: response[img].split(';'),
        })
      }
    }

    this.setState({
      images_with_results: images_with_results,
    })
  }

  render() {
    return (
      <div className="App" style={{textAlign: 'center'}}>
        <div style={{padding: "50px 0 10px 0"}}>
          <form onSubmit={this.handleSubmitFiles}>
            <input type="file" name="image" onChange={this.handleFileSelect} />
            <input type="submit" value="Send"/>
          </form>
        </div>
        <div style={{padding: "10px 0 50px 0"}}>
        <select
          value={this.state.selectedEmotion}
          onChange={this.handleEmotionSelect}
        >
          <option value="">Sort by</option>
          {EMOTIONS.map(emotion => <option value={emotion}>{emotion}</option>)}
        </select>
        </div>
        <table style={{marginLeft: 'auto', marginRight: 'auto'}}>
        <thead>
          <tr>
              <th scope="col">Image</th>
              <th scope="col">Result</th>
          </tr>
        </thead>
        <tbody>
          {this.state.images_with_results.map((image_with_result) => 
            <tr>
              <td style={{padding: 20}}>
              <img
                height="300"
                style={{display: 'block', margin: 'auto'}}
                src={`data:image/jpeg;base64,${image_with_result['image']}`}
              />
              </td>
              <td style={{padding: 20}}>
                <div style={{width: 300}}>
                {image_with_result['result'].map(str => <span>
                  {(this.state.selectedEmotion !== "" && str.includes(this.state.selectedEmotion)) ?
                    <b>{str}</b>
                  :
                    <span>{str}</span>
                  }
                  <br />
                </span>)
                }
                </div>
              </td>
            </tr>
          )}
        </tbody>
        </table>
      </div>
    );
  }
}

export default App;
